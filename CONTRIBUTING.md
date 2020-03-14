# GUTSchedule Contribution Guidelines
Welcome, and thank you for your interest in contributing to my app!

There are many ways in which you can contribute, beyond writing code. The goal of this document is to provide a high-level overview of how you can get involved.

## Table of Contents
- [GUTSchedule Contribution Guidelines](#gutschedule-contribution-guidelines)
  - [Table of Contents](#table-of-contents)
  - [Asking Questions](#asking-questions)
  - [Providing Feedback](#providing-feedback)
  - [Reporting Issues](#reporting-issues)
    - [Look For an Existing Issue](#look-for-an-existing-issue)
    - [Writing Good Bug Reports and Feature Requests](#writing-good-bug-reports-and-feature-requests)
    - [Final Checklist](#final-checklist)
    - [Follow Your Issue](#follow-your-issue)
  - [Contributing to codebase](#contributing-to-codebase)
    - [Build and run project](#build-and-run-project)
    - [Development workflow](#development-workflow)
      - [Release](#release)
    - [Coding guidelines](#coding-guidelines)
      - [Indentation](#indentation)
      - [Names](#names)
      - [Comments](#comments)
      - [Strings](#strings)
      - [Style](#style)
    - [Finding an issue to work on](#finding-an-issue-to-work-on)
    - [Contributing to translations](#contributing-to-translations)
    - [Submitting pull requests](#submitting-pull-requests)
      - [Spell check errors](#spell-check-errors)
  - [Thank You!](#thank-you)
  - [Attribution](#attribution)

## Asking Questions
Have a question? Rather than opening an issue, please ask me directly on opensource@xfox111.net.

## Providing Feedback
Your comments and feedback are welcome.
You can leave your feedbak on my app on feedback@xfox111.net or leave a review on [Google Play](https://play.google.com/store/apps/details?id=com.xfox111.gut.schedule)

## Reporting Issues
Have you identified a reproducible problem in the application? Have a feature request? I'd like to hear it! Here's how you can make reporting your issue as effective as possible.

### Look For an Existing Issue
Before you create a new issue, please do a search in [open issues](https://github.com/xfox111/gutschedule/issues) to see if the issue or feature request has already been filed.

Be sure to scan through the [feature requests](https://github.com/XFox111/GUTSchedule/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement).

If you find your issue already exists, make relevant comments and add your [reaction](https://github.com/blog/2119-add-reactions-to-pull-requests-issues-and-comments). Use a reaction in place of a "+1" comment:

* 👍 - upvote
* 👎 - downvote

If you cannot find an existing issue that describes your bug or feature, create a new issue using the guidelines below.

### Writing Good Bug Reports and Feature Requests
File a single issue per problem and feature request. Do not enumerate multiple bugs or feature requests in the same issue.

Do not add your issue as a comment to an existing issue unless it's for the identical input. Many issues look similar, but have different causes.

The more information you can provide, the more likely someone will be successful at reproducing the issue and finding a fix.

Please include the following with each issue:
- Current version of the app
- Your current Android version
- Reproducible steps (1... 2... 3...) that cause the issue
- What you expected to see, versus what you actually saw
- Images, animations, or a link to a video showing the issue occurring

### Final Checklist
Please remember to do the following:
- [ ] Search the issue repository to ensure your report is a new issue
- [ ] Separate issues reports
- [ ] Include as much information as you can to your report

Don't feel bad if the developers can't reproduce the issue right away. They will simply ask for more information!

### Follow Your Issue
Once your report is submitted, be sure to stay in touch with the devs in case they need more help from you.

## Contributing to codebase
If you are interested in writing code to fix issues or implement new awesome features you can follow this guidelines to get a better result

### Build and run project
1. Clone repository to local storage using [Git command prompt](https://guides.github.com/introduction/git-handbook/) or [Visual Studio](https://docs.microsoft.com/en-us/visualstudio/get-started/tutorial-open-project-from-repo?view=vs-2019)
    - Git clone command:
    ```
    git clone https://github.com/xfox111/GUTSchedule.git
    ```
2. Open `GUT.Schedule/GUT.Schedule.sln` using [Microsoft Visual Studio](https://visualstudio.microsoft.com/) 2019 or later
    - Make sure you have properly installed and congigured [Xamarin.Android](https://docs.microsoft.com/en-us/xamarin/android/get-started/installation/) and UWP development environments

**Android**

1. In `Solution Explorer` right-click `GUTSchedule.Droid` project and click `Set as StartUp Project`
2. On the VS toolbar choose build configuration:
    - `Debug` for building and deploying test version of the app
    - `Release` for building final version of application and creating AppBundle package (.aab)
    - `Release (APK)` for building final version of application and creating Android package fike (.apk)
3. To ensure that code compiles go to `Build -> Build Solution`
4. [Debug application](https://docs.microsoft.com/en-us/xamarin/android/deploy-test/debugging/) on an actual device or Android Emulator

**UWP**

1. In `Solution Explorer` right-click `GUTSchedule.UWP (Universal Windows)` project and click `Set as StartUp Project`
2. On the VS toolbar choose build configuration:
    - `Debug` for building and deploying test version of the app
    - `Release` for building final version of application and creating Appx packages
3. To ensure that code compiles go to `Build -> Build Solution`
4. [Debug application](https://docs.microsoft.com/en-us/windows/uwp/debug-test-perf/deploying-and-debugging-uwp-apps) on your machine or Windows Device Emulator

### Development workflow
This section represents how contributors should interact with codebase implementing features and fixing bugs
1. Getting assigned to the issue
2. Creating a repository fork
3. Making changes to codebase
4. Updating [changelog.md](changelog.md)
5. Creating a pull request to `master`
6. Validating PR with CI build
7. Completing PR
8. Done
#### Release
Next stage is release. Release performs on every push to master (which makes functional changes to application source code). Release performs via Azure Pipelines and triggers manually by pipeline owner ([@xfox111](https://github.com/xfox111)).

### Coding guidelines
#### Indentation
We use tabs, not spaces.

#### Names
The project naming rules inherit [.NET Naming Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines). Nevertheless there'is some distinction with the guidelines as well as additions to the one:
- Use `camelCase` for fields instead of `CamelCase` stated in [Capitalization Conventions](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions#capitalization-rules-for-identifiers)
- `camelCase` for resource keys is allowed but not recommeded. Consider using a `PascalCase`
- Private fields for properties should always start with underscore `_`
    - Wrong:
        ```
        private int year = 1984;
        public int Year 
        {
            get => year;
            set => year = value;
        }
        ```
    - Correct:
        ```
        private int _year = 1984;
        public int Year 
        {
            get => _year;
            set => _year = value;
        }
        ```
> **Note:** underscores `_` before generic **private** fields are allowed but not recommended
- Use `PascalCase` for file names
> **Note:** `snake_case` is allowed only for XML files and media resource files (`.png, .svg, .mp3, etc`), but still not recommended

#### Comments
Read [XML documentation comments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/) and try to use all stated methods. Remember: the more detailed documentation your code has the less programmers will curse you in the future

#### Strings
Use "double quotes" wherever it's possible
#### Style
- Prefer to use lambda functions
    - Wrong:
        ```
        button.Click += Button_Click;
        ...
        private void Button_Click (object sender, RoutedEventArgs e) 
        {
            Console.WriteLine("Hello, World!");
        }
        ```
        ```
        public void Main () 
        {
            Console.WriteLine("Hello, World!");
        }
        ```
    - Correct:
        ```
        button.Click += (s, e) => Console.WriteLine("Hello, World!");
        ```
        ```
        public void Main () =>
            Console.WriteLine("Hello, World!");
        ```
- Put curly braces on new lines
    - Wrong:
        ```
        if (condition) {
            ...
        }
        ```
    - Correct:
        ```
        if (condition)
        {
            ...
        }
        ```
- Put spaces between operators and before braces in methods declarations, conditionals and loops
    - Wrong: 
        - `y=k*x+b`
        - `public void Main()`
    - Correct:
        - `y = k * x + b`
        - `public void Main ()`
- Put `private` keyword even though it's unnecessary
    - Wrong: `void Main ()`
    - Correct: `private void Main ()`
- Use interpolated strings and ternary conditionals wherever it's possible
    - Wrong: 
        - `string s = a + "; " + b`, `string s = string.Format("{0}; {1:00}", a, b)`
        - ```
            string s;
            if (condition)
                s = "Life";
            else
                s = "Death"
            ```
    - Correct:
        - `string s = $"{a}; {b:00}"`
        - `string s = condition ? "Life" : "Death"`
- Do not surround loop and conditional bodies with curly braces if they can be avoided
    - Wrong:
        ```
        if (condition)
        {
            Console.WriteLine("Hello, World!");
        }
        else
        {
            return;
        }
        ```
    - Correct
        ```
        if (condition)
            Console.WriteLine("Hello, World!");
        else
            return;
        ```
- Use `#region` tags to separate code which does routine work (e.g. assigns Android layout controls or subscribes them to events)

### Finding an issue to work on
Check out the [full issues list](https://github.com/XFox111/GUTSchedule/issues?utf8=%E2%9C%93&q=is%3Aopen+is%3Aissue) for a list of all potential areas for contributions. **Note** that just because an issue exists in the repository does not mean we will accept a contribution to the core editor for it. There are several reasons we may not accept a pull request like:

- Performance - One of GUT.Schedule core values is to deliver a lightweight application, that means it should perform well in both real and perceived performance.
- User experience - Since we want to deliver a lightweight application, the UX should feel lightweight as well and not be cluttered. Most changes to the UI should go through the issue owner and.
- Architectural - Feature owner needs to agree with any architectural impact a change may make. Such things must be discussed with and agreed upon by the feature owner.

To improve the chances to get a pull request merged you should select an issue that is labelled with the `help-wanted` or `bug` labels. If the issue you want to work on is not labelled with `help-wanted` or `bug`, you can start a conversation with the issue owner asking whether an external contribution will be considered.

To avoid multiple pull requests resolving the same issue, let others know you are working on it by saying so in a comment.

### Contributing to translations
If you want to help us to translate this application into other languages you can do it making these steps:

**Android**

1. Navigate to `GUT.Schedule/GUTSchedule.Droid/Resources`
2. Create new folder named `values-lc` where `lc` is locale language.
Folder name examples:
   - `values-uk` for Ukrainian
   - `values-be` for Belarusian
   - `values-ru` for Russian

    For reference, here is a [list of Java locales](https://www.oracle.com/technetwork/java/javase/locales-137662.html) and [Android-supported locales via StackOverflow](https://stackoverflow.com/questions/7973023/what-is-the-list-of-supported-languages-locales-on-android).

3. In the folder copy `strings.xml` file from `GUT.Schedule/GUTSchedule.Droid/Resources/values`

**UWP**

1. Navigate to `GUT.Schedule/GUTSchedule.UWP/Strings`
2. Clone the folder you want to base your localization on and change its name to match desired localization language. See [Globalization and localization](https://docs.microsoft.com/en-us/windows/uwp/design/globalizing/globalizing-portal) for more information
3. Open `.resw` files from the folder in Visual Studio or other text editor
***
4. Edit localization strings
5. Commit changes and create a pull request

> **Note** that we only accept region neutral localizations (e.g. pull request with `values-en-rUS` (English (United States)) folder will be rejected whether PR with `values-en` (English) folder will be accepted).

### Submitting pull requests
To enable us to quickly review and accept your pull requests, always create one pull request per issue and [link the issue in the pull request](https://github.com/blog/957-introducing-issue-mentions). Never merge multiple requests in one unless they have the same root cause. Be sure to follow our [Coding Guidelines](#coding-guidelines) and keep code changes as small as possible. Avoid pure formatting changes to code that has not been modified otherwise. Pull requests should contain tests whenever possible. Fill pull request content according to its template. Deviations from template are not recommended

#### Spell check errors
Pull requests that fix spell check errors in translatable strings (strings in nls.localize(...) calls) are welcomed but please make sure it doesn't touch multiple feature areas, otherwise it will be difficult to review. Pull requests only fixing spell check errors in source code are not recommended.

## Thank You!

Your contributions to open source, large or small, make great projects like this possible. Thank you for taking the time to contribute.

## Attribution
This Contribution Guidelines are adapted from the [Contributing to VS Code](https://github.com/microsoft/vscode/blob/master/CONTRIBUTING.md)