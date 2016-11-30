namespace Atlas.Drawing
{
    public class Bitmap : Image
    {
        public Bitmap(int width, int height) : base(new byte[width*height*4], width, height)
        {
            
        }
    }
}