using Microsoft.Extensions.Configuration;

namespace Speaker.Recorder.ViewModels
{
    public class DesignRecordViewModel : RecordViewModel
    {
        private static IConfiguration configuration;

        static DesignRecordViewModel()
        {
            configuration = new ConfigurationBuilder()
                .Build();
        }

        public DesignRecordViewModel() : base(null, null, null, null, null, configuration, null)
        {
        }
    }
}
