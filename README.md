# Burger King Bot (bkbot)

`bkbot` is a C# written bot that will emulate an offscreen web browser in order to get a promotional code for your Burger King ticket. Promotional codes are given by Burger King when completing their survey.

[![GitHub release](https://img.shields.io/github/release/hawezo/BurgerKingBot.svg?style=flat-square)](https://github.com/hawezo/BurgerKingBot)
[![GitHub issues](https://img.shields.io/github/issues/hawezo/BurgerKingBot.svg?style=flat-square)](https://github.com/hawezo/BurgerKingBot/issues)
[![Github downloads](https://img.shields.io/github/downloads/hawezo/BurgerKingBot/total.svg?style=flat-square)](https://github.com/hawezo/BurgerKingBot)

![](https://i.imgur.com/DGvS5pK.gif)

# Installation

As this bot is written in C#, it will only work on Windows. It uses .NET Framework 4.6.1.  
All you have to do is to download [this archive](http://hawezo.legtux.org/downloads/bkbot.zip) (dead link atm) which contains a `bin` folder with the bot and its files, and two `.cmd` scripts. The first one will start the bot and the second one will open the `settings.json` file used by the bot.

## Settings

Settings are stored in a `settings.json` file which has to be in the same directory as `bkbot.exe`.

```json
{  
   "url":"http://bkvousecoute.fr/",
   "reference":"23143",
   "postalCode":"13490",
   "amount":2
}
```

`url`: the URL of the survey. Other localization may work as well, but have not been tested.  
`reference`: the reference of your Burger King. It is written in the top front of your ticket.  
`postalCode`: required for the survey, any valid postal code.  
`amount`: the amount of codes the bot will try to get after starting it. 

## How it works

`bkbot` uses [CefSharp](https://github.com/cefsharp/CefSharp) to emulate a web browser and interact with it.

It will start by opening the URL in the `settings.json` file, and will then proceed step by step to complete the survey, using Javascript to fill the required data.

Thanks to CefSharp, using Javascript is quite simple:

```csharp
// fill the reference text box
await _browser.EvaluateScriptAsync($"document.getElementById('SurveyCode').value = '{_settings.reference}';");

// click the next button
await _browser.EvaluateScriptAsync("document.getElementById('NextButton').click();");
```
When at the end of the survey, Burger King gives us the promotional code. Grabbing it is as simple:

```csharp
JavascriptResponse js =
    await _browser.EvaluateScriptAsync($"document.getElementsByClassName('ValCode')[0].innerHTML;");
this.Code = js.Result.ToString().Split(' ')[js.Result.ToString().Split(' ').Length - 1].Trim();
```

And yes, I know, I could have put more efforts and less redundancy in parsing the code from the string.

## Libraries used

- [CefSharp](https://github.com/cefsharp/CefSharp): used to emulate an offscreen web browser thanks to Chromium. Its downside is its size, about 60 Mo.
- [ShellProgressBar](https://github.com/Mpdreamz/shellprogressbar): used to display a fancy progress bar.

---

![](https://i.imgur.com/FLrDZDt.png)
