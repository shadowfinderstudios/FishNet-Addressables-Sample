namespace Shadowfinder.Gathering
{
    public interface IMineable
    {
        public void ResetResource();
        public int MineResource(int damage);
    }
}
