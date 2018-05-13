using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/*
EXAMPLE PATCH JSON:
{
	"rootDirectory": "D:\\SomeDirectory\\SomeOtherDirectory",
	"targets": [
		{
			"targetFileName": "someRandomSettingsFile.json",
			"patches": [
				{
					"fieldName": "SomeJsonField",
					"action": "Replace",
					"patchedValue": "ThisHasChanged"
				},
				{
					"fieldName": "SomeOtherJsonField",
					"action": "Replace",
					"patchedValue": "ThisHasAlsoChanged"
				}
			]
		}
	]
}
*/

namespace JsonPatcher
{
    public class JsonPatchFile
    {
        public string rootDirectory { get; set; }
        public JsonPatchFileTarget[] targets { get; set; }
    }

    public class JsonPatchFileTarget
    {
        public string targetFileName { get; set; }
        public JsonFieldPatch[] patches { get; set; }
    }

    public class JsonFieldPatch
    {
        public string fieldName { get; set; }
        public string action { get; set; }
        public string patchedValue { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("*** PATCH JSON ***");

            int numArgs = args.Length;
            Console.WriteLine("Arg count " + numArgs);
            if (numArgs < 1)
            {
                Console.WriteLine("Please provide:");
                Console.WriteLine("A json file specifying patch instructions");
                Console.WriteLine("Optional: a path to the root search directory, will override path specified in patch json.");
                Console.WriteLine("Will use exe directory if no path is found in patch json or command line.");
                Console.WriteLine("Command line: PatchJson MyPatchInstructions.json C:\\someDirectory\\RootOfSearch");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                return;
            }

            string patchFilePath = args[0];
            string searchRootDirectory = string.Empty;
            bool rootPathOverride = false;
            if (numArgs == 1)
            {
                searchRootDirectory = Directory.GetCurrentDirectory();
            }
            else
            {
                searchRootDirectory = args[1];
                rootPathOverride = true;
            }

            Console.WriteLine("Using patch file " + args[0]);

            // Load patch settings

            StreamReader reader = null;
            try
            {
                reader = File.OpenText(args[0]);
                JsonSerializer serialiser = new JsonSerializer();
                JsonPatchFile patchFile = (JsonPatchFile)serialiser.Deserialize(reader, typeof(JsonPatchFile));

                string rootDirLowered = patchFile.rootDirectory.ToLower();
                if (string.IsNullOrWhiteSpace(rootDirLowered)
                    || rootDirLowered == "root"
                    || rootPathOverride == true)
                {
                    patchFile.rootDirectory = searchRootDirectory;
                }

                Console.WriteLine("Patch target count: " + patchFile.targets.Length);
                Console.WriteLine("Search Root: " + patchFile.rootDirectory);

                // Loop patch file targets
                foreach (JsonPatchFileTarget target in patchFile.targets)
                {
                    Console.WriteLine("\nSearching for target " + target.targetFileName);
                    List<string> targetPaths = new List<string>(100);
                    FindPatchTargets(patchFile.rootDirectory, target, targetPaths);
                    Console.WriteLine("\nFound " + targetPaths.Count + " target files.");

                    foreach (string path in targetPaths)
                    {
                        Console.WriteLine("==================================================");
                        Console.WriteLine("Searching file '" + path + "'");
                        int fieldsPatched = ParseTarget(path, target.patches);
                        Console.WriteLine("Patched " + fieldsPatched + " fields");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Well, something cocked up");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (reader != null) { reader.Close(); }
            }
            Console.WriteLine("\nDone");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            return;
        }

        /***************************************************************************************
         * Build file list
         ***************************************************************************************/
        static void FindPatchTargets(string rootDirectory, JsonPatchFileTarget target, List<string> targetPaths)
        {
            // Look for target files in current directory
            string[] files = Directory.GetFiles(rootDirectory, "*.json");
            int numFiles = files.Length;

            foreach (string filePath in files)
            {
                if (Path.GetFileName(filePath) == target.targetFileName
                    && targetPaths.Contains(filePath) == false)
                {
                    targetPaths.Add(filePath);
                }
            }

            // Then scan sub directories
            string[] directories = Directory.GetDirectories(rootDirectory);
            int numDirectories = directories.Length;
            foreach (var dir in directories)
            {
                FindPatchTargets(dir, target, targetPaths);
            }
        }

        static void WriteJsonObject(string filePath, JObject obj)
        {
            /**
             * > StreamWriter requires a dedicated file stream in order to force the output to UTF-8
             * > FileMode must be Create because otherwise modified Json will be inserted into the current file
             * rather than replacing what is there (which will most likely just corrupt the json anyway)
             * > Set formatting to indented for readability... consider a switch to disable this?
             */
            using (StreamWriter writer = new StreamWriter(
                new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite),
                Encoding.UTF8
                ))
            {
                JsonSerializer serialiser = new JsonSerializer();
                serialiser.Formatting = Formatting.Indented;
                serialiser.Serialize(writer, obj);
            }
        }

        /***************************************************************************************
         * Patch file
         ***************************************************************************************/
        static int ParseTarget(string targetJsonFilePath, JsonFieldPatch[] patches)
        {
            //Console.WriteLine("Search for fields in " + targetJsonFilePath);
            string jsonLiteral;
            using (StreamReader r = new StreamReader(targetJsonFilePath))
            {
                jsonLiteral = r.ReadToEnd();
            }

            JObject jsonObject = JObject.Parse(jsonLiteral);
            //Console.WriteLine("Root node type: " + jsonObject.Type);
            int fieldsPatched = SearchForFields(jsonObject, patches, "Root");

            // construct new file name for testing!
            //string pathNoFileName = Path.GetDirectoryName(targetJsonFilePath);
            //string testFileName = @"patchedtest.json";
            //string savePath = pathNoFileName + testFileName;
            string savePath = targetJsonFilePath;

            WriteJsonObject(savePath, jsonObject);

            return fieldsPatched;
        }

        static int SearchForFields(JToken obj, JsonFieldPatch[] patches, string nodeLabel)
        {
            int fieldsPatched = 0;

            IEnumerable<JToken> children = obj.Children<JToken>();
            foreach (JToken child in children)
            {
                JProperty prop = child as JProperty;
                string propName = prop != null ? prop.Name : string.Empty;
                JContainer container = child as JContainer;
                int childCount = container != null ? container.Count : 0;

                //Console.WriteLine("In '" + nodeLabel + "' Child: '" + propName + "' child count: " + childCount + " type: " + child.Type);

                foreach (JsonFieldPatch patch in patches)
                {
                    if (string.IsNullOrWhiteSpace(propName) == false && propName == patch.fieldName)
                    {
                        //Console.WriteLine("MATCH on " + patch.fieldName);
                        if (ExecuteAction(prop, patch))
                        {
                            fieldsPatched++;
                        }
                    }
                }

                fieldsPatched += SearchForFields(child, patches, propName);
            }
            return fieldsPatched;
        }

        static bool ExecuteAction(JProperty field, JsonFieldPatch patch)
        {
            //Console.WriteLine("Patching " + field.Name + " to " + patch.patchedValue);
            Console.WriteLine("Patching " + field.Name);
            field.Value = patch.patchedValue;
            return true;
        }
    }
}
