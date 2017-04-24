namespace Pub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using CommandLine;
    using CommandLine.Text;
    
    class Options : CommandLineOptionsBase
    {
        [Option("n", "totalMessages", Required = false, HelpText = "Total no. of messages per topic. Default is 1 per topic")]
        public long totalMessages { get; set; }

        [Option("q", "messageQueue", Required = false, HelpText = "Message sending Queue")]
        public string messageQueue { get; set; }

        [HelpOption(HelpText = "Display on help screen.")]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = "Publisher",
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            this.HandleParsingErrorsInHelp(help);
            help.AddPreOptionsLine("Usage: Pub.exe [-n <max nb msg>] [-q <messagequeue>]");
            help.AddOptions(this);

            return help;
        }

        private void HandleParsingErrorsInHelp(HelpText help)
        {
            if (this.LastPostParsingState.Errors.Count > 0)
            {
                var errors = help.RenderParsingErrorsText(this, 2);
                if (!string.IsNullOrEmpty(errors))
                {
                    help.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));
                    help.AddPreOptionsLine(errors);
                }
            }
        }

        public Options()
        {
            totalMessages = 1;
        }
    }
}
