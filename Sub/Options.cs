namespace Sub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using CommandLine;
    using CommandLine.Text;

    class Options : CommandLineOptionsBase
    {
        [OptionList("s", "subscribedTopics", Required = false, Separator = ';', HelpText = "List of topics filters seperated by ';'. Default is empty")]
        public IList<string> subscribedTopics { get; set; }

        [Option("q", "messageQueue", Required = false, HelpText = "Message sending Queue")]
        public string messageQueue { get; set; }

        [HelpOption(HelpText = "Display on help screen.")]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = "Subscriber",
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            this.HandleParsingErrorsInHelp(help);
            help.AddPreOptionsLine("Usage: Sub.exe [-s <subscribed topics>] [-q <messagequeue>]");
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
            subscribedTopics = new List<string>();
        }
    }
}
