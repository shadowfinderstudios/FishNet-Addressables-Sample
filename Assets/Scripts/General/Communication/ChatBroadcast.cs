using FishNet.Broadcast;

namespace Shadowfinder.Communication
{
    public struct ChatBroadcast : IBroadcast
    {
        public string Username;
        public string Message;
    }
}
