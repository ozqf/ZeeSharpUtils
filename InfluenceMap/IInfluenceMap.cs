
namespace ZqfInfluenceMap
{
    public interface IInfluenceMap
    {
        int QueueBlit(Rect r, float val);

        int width { get; }
        int height { get; }

        int QueueQueryForBestZero(int originX, int originY);

        int QueueSetPixel(int x, int y, float val);

        void Process();

        float[] GetValuesCopy();

        string DebugPrint();
    }
}
