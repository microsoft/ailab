namespace SnipInsight.Forms.Common
{
    public class Messenger
    {
        private static Messenger instance;

        private Messenger()
        {
        }

        public static Messenger Instance => instance ?? (instance = new Messenger());
    }
}
