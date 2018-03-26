using CefSharp;
using CefSharp.OffScreen;
using ShellProgressBar;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace bkbot.Core
{
    public class Bot
    {
        public string Code { private set; get; }
        public string Error { private set; get; }
        public BotStep Step { private set; get; }
        public bool DisplayProgress
        {
            get { return _displayProgress; }
        }
        public int CurrentStep
        {
            get
            {
                return ((int)this.Step);
            }
        }
        public int Steps {
            get
            {
                return Enum.GetNames(typeof(BotStep)).Length;
            }
        }
        public bool HasError
        {
            get
            {
                return this.Error != null;
            }
        }

        private ProgressBar _progress;
        private ProgressBarOptions _options;
        private ChromiumWebBrowser _browser;
        private BotSettings _settings;
        private bool _displayProgress;

        /// <summary>
        /// Constructor for the code fetching bot
        /// </summary>
        /// <param name="displayProgress">Define wether or not progress is displayed (fills console)</param>/param>
        public Bot(BotSettings settings, bool displayProgress = true)
        {
            // loading settings
            _settings = settings;
            _displayProgress = displayProgress;
        }

        /// <summary>
        /// Enum defining every step of the code fetching process
        /// </summary>
        public enum BotStep
        {
            CookieValidation,
            Reference,
            Step1,
            Step2,
            Step3,
            Step4,
            Step5,
            Step6,
            Step7,
            Step8,
            Step9,
            Step10,
            Step11,
            Step12,
            Step13,
            Step14,
            Step15,
            Step16,
            Step17,
            Step18,
            Step19,
            Step20
        }

        #region Public Methods

        /// <summary>
        /// Gets a burger king bot
        /// </summary>
        /// <returns></returns>
        public string GetCode()
        {
            this.Error = null;
            this.Code = null;
            this.Step = BotStep.CookieValidation;

            // setting up bot
            if (_displayProgress)
            {
                _options = new ProgressBarOptions
                {
                    ProgressCharacter = '/',
                    ProgressBarOnBottom = true,
                    CollapseWhenFinished = true,
                    ForegroundColor = ConsoleColor.Gray,
                    BackgroundColor = ConsoleColor.DarkGray,
                    ForegroundColorDone = ConsoleColor.DarkGreen,
                };
                if (_progress != null) _progress.Dispose();
                _progress = new ProgressBar(
                    this.Steps, // ticks
                    "Starting", // intial text
                    new ProgressBarOptions
                    {
                        ProgressCharacter = '\\',
                        ProgressBarOnBottom = true,
                        CollapseWhenFinished = true,
                        ForegroundColor = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.DarkCyan,
                    }); // options
            }

            Task task = Task.Run(async () => {
                this.StartFetching();
                do
                    await Task.Delay(1000);
                while (this.Code == this.Error);
            });
            task.Wait();
            
            return (this.Code ?? this.Error);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Starts fetching the code
        /// </summary>
        private void StartFetching()
        {
            _browser = new ChromiumWebBrowser(_settings.url);

            // registering js event
            if (_displayProgress) _progress.Tick("Registering Javascript events");
            _browser.LoadingStateChanged += this.Browser_PageLoadedAsync;
        }

        /// <summary>
        /// Event triggered when page is loaded
        /// </summary>
        private async void Browser_PageLoadedAsync(object sender, LoadingStateChangedEventArgs e)
        {
            await Task.Delay(250);
            if (!e.Browser.IsDisposed && _browser.IsBrowserInitialized)
            {
                if (_displayProgress) _progress.Message = $"{this.CurrentStep} of {this.Steps} - analyzing page";
                if (!e.IsLoading)
                    try {
                        await this.HandleStepAsync();
                    }
                    catch (Exception)
                    {
                        if (_displayProgress) _progress.Message = $"{this.CurrentStep} of {this.Steps} - fetching stopped";
                    }
                if (_displayProgress) _progress.Message = $"{this.CurrentStep} of {this.Steps} - waiting for next event";
            }
            else
            {
                if (_displayProgress) _progress.Message = $"{this.CurrentStep} of {this.Steps} - fetching stopped";
            }
        }

        /// <summary>
        /// Handles every step of the fetching process
        /// </summary>
        /// <returns></returns>
        private async Task HandleStepAsync()
        {
            // dereferencing event
            _browser.LoadingStateChanged -= this.Browser_PageLoadedAsync;
            if (_displayProgress) _progress.Tick($"{this.CurrentStep} of {this.Steps} - In Progress ({this.Step.ToString()})");

            await Task.Delay(250);

            // switching through steps
            string source = await this.GetSourceAsync();
            switch (this.Step)
            {

                case BotStep.CookieValidation:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, "Agreeing with cookie policy", _options);
                        do
                        {
                            if (_displayProgress) child.Message = "Unexpected page - trying again in 3 seconds";
                            _browser.Load(_settings.url);
                            await Task.Delay(3000);
                        }while (!(await this.PageContainsAsync("NextButton")));
                        if (_displayProgress) child.Tick("Agreed with cookie policy");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Reference:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(2, "Acquainting reference and dates", _options);
                        do
                        {
                            if (_displayProgress) child.Message = "Unexpected page - trying again in 3 seconds";
                            await Task.Delay(3000);
                        } while (!(await this.PageContainsAsync("NextButton", "SurveyCode")));
                        
                        await _browser.EvaluateScriptAsync($"document.getElementById('SurveyCode').value = '{_settings.reference}';");
                        if (_displayProgress) child.Tick("Injected Burger King reference identifier");
                        
                        DateTime date = this.GenerateRandomDate();
                        if (_displayProgress) child.Message = $"Generated random date: {date.ToString()}";
                        await Task.Delay(500);

                        await _browser.EvaluateScriptAsync($"document.getElementById('InputDay').value = '{date.ToString("dd")}';");
                        await _browser.EvaluateScriptAsync($"document.getElementById('InputMonth').value = '{date.ToString("MM")}';");
                        await _browser.EvaluateScriptAsync($"document.getElementById('InputHour').value = '{date.ToString("hh")}';");
                        await _browser.EvaluateScriptAsync($"document.getElementById('InputMinute').value = '{date.ToString("mm")}';");
                        if (_displayProgress) child.Tick($"Injected date: {date.ToString()}");

                        await this.NextStepAsync(child);
                        await Task.Delay(500);

                        if (await this.PageContainsAsync("Error"))
                        {
                            if (_displayProgress) child.Message = "Invalid reference";
                            this.Abort("The specified reference is not recognized by the survey engine.");
                        }
                        break;
                    }

                case BotStep.Step1:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R001000.2");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step2:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R000019.1");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step3:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R002000.1");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step4:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R004000.5");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step5:
                case BotStep.Step6:
                case BotStep.Step7:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.SelectSatisfactionCriteria(child,
                            "R012000",
                            "R017000",
                            "R107000",
                            "R020000",
                            "R009000",
                            "R000020",
                            "R015000",
                            "R008000",
                            "R000063",
                            "R013000",
                            "R011000",
                            "R000020",
                            "R023000");
                        await this.NextStepAsync(child);
                        await Task.Delay(500);

                        if (await this.PageContainsAsync("FNSleftBlank"))
                        {
                            if (_displayProgress) child.Message = "Survey likely changed";
                            this.Abort("The survey must have changed quite a bit due to an unexpected question.");
                        }
                        break;
                    }

                case BotStep.Step8:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(2, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R029000.5", "R030000.5");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step9:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R041000.2");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step10:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(2, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R044000.5", "R045000.5");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step11: // big box
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(0, $"Filling page {this.CurrentStep}", _options);
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step12:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R000091");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step13:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R000097");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step14:
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R049000.1");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step15: // amount of time came to bk
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R057000.1");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step16: // reason
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R060000.9");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step17: // others
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling page {this.CurrentStep}", _options);
                        await this.ClickElements(child, "R068000.9");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step18: // last, dropdowns
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(2, $"Changing dropdowns on page {this.CurrentStep}", _options);
                        await this.SetDropdownValues(child, 3, "R069000", "R070000");
                        await this.NextStepAsync(child);
                        break;
                    }

                case BotStep.Step19: // postal code
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Filling postal code on page {this.CurrentStep}", _options);
                        await _browser.EvaluateScriptAsync($"document.getElementById('S076000').value = '{_settings.postalCode}';");
                        if (_displayProgress) child.Tick($"Injected postal code: {_settings.postalCode}");
                        await this.NextStepAsync(child);
                        break;
                    }
                    
                case BotStep.Step20: // retrieve promo code
                    {
                        ChildProgressBar child = !_displayProgress ? null : _progress.Spawn(1, $"Parsing promo code", _options);
                        JavascriptResponse js = await _browser.EvaluateScriptAsync($"document.getElementsByClassName('ValCode')[0].innerHTML;");
                        this.Code = js.Result.ToString().Split(' ')[js.Result.ToString().Split(' ').Length - 1].Trim();
                        await this.NextStepAsync(child);
                        _progress.Message = $"Finished fetching code: {this.Code}";
                        _browser.LoadingStateChanged -= this.Browser_PageLoadedAsync;
                        break;
                    }
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Generate a random date in order to fill the survey
        /// </summary>
        private DateTime GenerateRandomDate()
        {
            Random random = new Random();
            DateTime date = new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                random.Next(11, 15),
                random.Next(0, 59),
                random.Next(0, 59));
            return date.AddDays(-random.Next(1, 15));
        }

        /// <summary>
        /// Aborts the fetching and return an error
        /// </summary>
        private async void Abort(string reason)
        {
            this.Error = reason;
            try
            {
                _browser.LoadingStateChanged -= this.Browser_PageLoadedAsync;
                _browser.Stop();
                if (_displayProgress) _progress.Dispose();
            }
            catch (Exception) { }
            
            await Task.Delay(500);
            _browser.Dispose();
        }

        #endregion

        #region Browser Methods

        /// <summary>
        /// Go to next step
        /// </summary>
        private async Task NextStepAsync(ChildProgressBar child)
        {
            await _browser.EvaluateScriptAsync("document.getElementById('NextButton').click();");
            this.Step++;
            if (_displayProgress) child.Tick($"{this.CurrentStep} of {this.Steps} - {child.Message} (step finished)");
            _browser.LoadingStateChanged += this.Browser_PageLoadedAsync;
            if (_displayProgress) child.Dispose();
        }

        /// <summary>
        /// Get the current source code
        /// </summary>
        private async Task<string> GetSourceAsync()
        {
            return _browser.IsBrowserInitialized ? await _browser.GetSourceAsync() : null;
        }

        /// <summary>
        /// Check if the current page contains a specific element
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        private async Task<bool> PageContainsAsync(params string[] elements)
        {
            string source = await this.GetSourceAsync();
            return elements.All((id) => source != null && (source.Contains($"id=\"{id}") || source.Contains($"class=\"{id}")));
        }

        /// <summary>
        /// Simulate clicks on given elements
        /// </summary>
        private async Task ClickElements(ChildProgressBar child, params string[] elements)
        {
            int clicks = 0;
            foreach (string element in elements)
            {
                if (!(await this.PageContainsAsync(element)))
                {
                    if (_displayProgress) child.Message = $"Missing expected element [{element}]";
                    this.Abort($"An expected element ({element}) was missing at step {this.Step.ToString()}.");
                }
                else
                {
                    await _browser.EvaluateScriptAsync($"document.getElementById('{element}').click()");
                    if (_displayProgress) child.Tick($"Clicked on element [{element}]");
                }
            }
            if (_displayProgress) child.Message = $"Clicked on {++clicks} element{(clicks > 1 ? "s" : null)}";
        }

        /// <summary>
        /// Simulate clicks on available criteria
        /// </summary>
        private async Task SelectSatisfactionCriteria(ChildProgressBar child, params string[] elements)
        {
            int clicks = 0;
            foreach (string element in elements)
            {
                if (await this.PageContainsAsync(element))
                {
                    await _browser.EvaluateScriptAsync($"document.getElementById('{element}.5').click()");
                    if (_displayProgress) child.MaxTicks += 1;
                    if (_displayProgress) child.Tick($"Clicked on element [{element}]");
                }
            }
            if (_displayProgress) child.Message = $"Clicked on {++clicks} element{(clicks > 1 ? "s" : null)}";
        }
        
        /// <summary>
        /// Changes dropdown values
        /// </summary>
        private async Task SetDropdownValues(ChildProgressBar child, int index = 2, params string[] elements)
        {
            int changes = 0;
            foreach (string element in elements)
            {
                if (await this.PageContainsAsync(element))
                {
                    await _browser.EvaluateScriptAsync($"document.getElementById('{element}').selectedIndex = {index};");
                    if (_displayProgress) child.Tick($"Changed dropdown [{element}] to value {index}");
                }
                else
                {
                    if (_displayProgress) child.Message = $"Missing expected dropdown [{element}] - execution stopped";
                    this.Abort("An expected dropdown was missing - either the survey changed or there was a network issue");
                }
            }
            if (_displayProgress) child.Message = $"Changed {++changes} dropdown{(changes > 1 ? "s" : null)}";
        }

        #endregion
    }
}
