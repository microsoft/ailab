using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Speaker.Recorder.Helpers
{
    public static class WaveFormatHelper
    {
        public static WaveFormat SelectBestWaveInFormat(this WaveInEvent waveIn)
        {
            var deviceCapabilities = WaveIn.GetCapabilities(waveIn.DeviceNumber);
            var formats = Enum.GetValues(typeof(SupportedWaveFormat)).Cast<SupportedWaveFormat>().OrderByDescending(x => x);
            foreach (var item in formats)
            {
                if (deviceCapabilities.SupportsWaveFormat(item))
                {
                    return item.ToWaveFormat();
                }
            }

            throw new NotSupportedException();
        }

        public static WaveFormat ToWaveFormat(this SupportedWaveFormat item)
        {
            var name = item.ToString();
            var match = Regex.Match(name, @"WAVE_FORMAT_(\d+)([MS])(\d+)");
            var rate = match.Groups[1].Value switch
            {
                "1" => 11025,
                "2" => 22050,
                "4" => 44100,
                "44" => 44100,
                "48" => 48000,
                "96" => 96000,
                _ => throw new NotImplementedException(),
            };
            var bits = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
            var channels = match.Groups[2].Value == "M" ? 1 : 2;
            return new WaveFormat(rate, bits, channels);
        }
    }
}
