namespace Server
{
    using System;
    using System.Text;
    using CommandLine;
    using CommandLine.Text;

    class Options : CommandLineOptionsBase
    {
        [Option("q", "messageQueue", Required = false, HelpText = "Message sending Queue")]
        public string messageQueue { get; set; }

        [HelpOption(HelpText = "Display on help screen.")]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = "Server",
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            this.HandleParsingErrorsInHelp(help);
            help.AddPreOptionsLine("Usage: Server.exe [-q <messagequeue>]");
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
    }
}
