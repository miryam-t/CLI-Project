using System.CommandLine;
using System.CommandLine.Parsing;

// 1. הגדרת האופציות
var languageOption = new Option<string[]>(
    name: "--language",
    description: "List of programming languages (java, html, cs, py, js) or 'all'"
    )
{ IsRequired=true,
 Arity = ArgumentArity.OneOrMore
};
languageOption.AddAlias("-l");
languageOption.AllowMultipleArgumentsPerToken = true;

var outputOption = new Option<FileInfo>(
    name: "--output",
    description: "File path and name")
{ IsRequired = true };
outputOption.AddAlias("-o");
    
var noteOption = new Option<bool>(
    name:"--note",
    description: "Include the source file path as a comment in the bundle");
noteOption.AddAlias("-n");

var sortOption = new Option<string>(
    name: "--sort",
    description: "Sort files by 'name' (alphabetical) or 'type' (extension)",
    getDefaultValue: () => "name");
sortOption.AddAlias("-s");

var removeEmptyLinesOption=new Option<bool>(
    name: "--remove-empty-lines",
    description:"Remove empty lines from the source code");
removeEmptyLinesOption.AddAlias("-r");

var authorOption=new Option<string>(
    name: "--author",
    description: "Name of the file creator to be listed at the top");
authorOption.AddAlias("-a");


// 2. הגדרת הפקודה והוספת האופציות אליה
var bundleCommand = new Command("bundle", "Bundle code files to a single file");
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);


bundleCommand.SetHandler((language, output,note, sort, removeEmptyLines, author) =>
{
    try
    {
        var allowedExtensions = new[] { "java", "html", "cs", "py", "js", "cpp", "c", "ts", "tsx", "jsx", "css", "json", "sql" };

        bool includeAll = language.Length == 1 &&
                          language[0].ToLower() == "all";

        if (!includeAll)
        {
            foreach (var lang in language)
            {
                if (!allowedExtensions.Contains(lang.ToLower()))
                {
                    Console.WriteLine($"Error: '{lang}' is not a supported language.");
                    return;
                }
            }
        }
        if (sort != "name" && sort != "type")
        {
            Console.WriteLine("Error: sort must be either 'name' or 'type'.");
            return;
        }

        // 1. זיהוי תיקיית העבודה הנוכחית (איפה שהמשתמש מריץ את הפקודה)
        var currentDirectory = Directory.GetCurrentDirectory();

        //2.איסוף כל הקבצים בתיקייה(כולל תתי - תיקיות, אך ללא תיקיות מוחרגות)
        var allFiles = Directory.GetFiles(currentDirectory, "*.*", SearchOption.AllDirectories);
        // 2. סינון קבצים לפי שפה ותיקיות אסורות
                var excludedDirectories = new[] { "bin", "debug", "obj", ".git", ".vs" };
        var filteredFiles = allFiles.Where(file =>
        {
            var directories = file.Split(Path.DirectorySeparatorChar);

            bool isInExcludedDir = directories
                .Any(d => excludedDirectories.Contains(d.ToLower()));

            if (isInExcludedDir)
                return false;

            var ext = Path.GetExtension(file).TrimStart('.').ToLower();

            if (includeAll)
                return true;

            return language.Select(l => l.ToLower()).Contains(ext);

        }).ToList();
    
        if (sort == "type")
        {
            filteredFiles = filteredFiles.OrderBy(f => Path.GetExtension(f)).ThenBy(f => Path.GetFileName(f)).ToList();
        }
        else // ברירת מחדל: לפי שם (א"ב)
        {
            filteredFiles = filteredFiles.OrderBy(f => Path.GetFileName(f)).ToList();
        }
        // 4. כתיבה לקובץ (שימוש ב-using var החדשני של C#)
        using var writer = new StreamWriter(output.FullName);

        if (!string.IsNullOrWhiteSpace(author))
            writer.WriteLine($"// Author: {author}");
        // 4. כתיבה לקובץ היעד

            foreach (var filePath in filteredFiles)
            {
                // הוספת הערה עם מקור הקוד אם המשתמש ביקש
                if (note)
                {
                    writer.WriteLine($"// Source: {Path.GetRelativePath(currentDirectory, filePath)}");
                }
                // קריאת תוכן הקובץ
            var lines = File.ReadLines(filePath);
            if (removeEmptyLines)
                lines = lines.Where(line => !string.IsNullOrWhiteSpace(line));
            foreach (var line in lines)
                {
                    writer.WriteLine(line);
                }
                writer.WriteLine(); // שורת רווח בין קבצים לנוחות הקריאה
            }
        foreach (var file in filteredFiles)
        {
            // כאן נמשיך בשלב הבא את הלוגיקה של כתיבת התוכן, מיון והסרת שורות
            Console.WriteLine($"Adding file: {Path.GetFileName(file)}");
        }

        Console.WriteLine($"Success: Bundle created at {output.FullName}");
        Console.WriteLine("Successfully created bundle file!");
  
    }
    catch (DirectoryNotFoundException) {
        Console.WriteLine("Error: The path provided for the output file is invalid.");
    }
    catch(Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }

}, languageOption, outputOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

//הסבר מפורט על הקוד:
//1.איסוף וסינון קבצים(LINQ)
//Directory.GetFiles: הפקודה הזו סורקת את התיקייה. הפרמטר SearchOption.AllDirectories אומר לה להיכנס גם לתוך תיקיות פנימיות.
//החרגת תיקיות: השתמשנו במערך excludedDirectories. הסינון נעשה בעזרת Path.DirectorySeparatorChar (הסימן \ או / תלוי במערכת ההפעלה) כדי לוודא שאנחנו פוסלים את התיקייה עצמה ולא קובץ שבמקרה מכיל את המילה "bin".
//בדיקת שפות: אנחנו מחלצים את הסיומת בעזרת Path.GetExtension ובודקים אם היא קיימת ברשימה שהמשתמש שלח, או אם המשתמש כתב all.
//2. המיון (Sorting)
//OrderBy: פונקציית LINQ שמאפשרת למיין רשימות.
//אם המשתמש בחר type, אנחנו ממינים קודם לפי הסיומת (GetExtension) ואז בתוך כל סיומת לפי שם הקובץ. אם לא, המיון הוא פשוט לפי שם הקובץ בלבד.
//3. הטיפול בתוכן הקבצים
//StreamWriter: זהו אובייקט שמאפשר כתיבה יעילה של טקסט לקובץ. ה-using מבטיח שהקובץ ייסגר ויישמר בבטחה גם אם תהיה שגיאה באמצע.
//Path.GetRelativePath: פקודה מעולה! היא הופכת נתיב ארוך (כמו C:\Users\Name\Project\Main.cs) לנתיב קצר שמתחיל מהתיקייה שבה אנחנו נמצאים (כמו Project\Main.cs). זה מה שהתבקש עבור ה-note.
//הסרת שורות ריקות: אנחנו קוראים את הקובץ שורה-שורה (ReadAllLines). אם האופציה removeEmptyLines פעילה, אנחנו משתמשים ב-string.IsNullOrWhiteSpace(line) כדי לדלג על שורות שאין בהן תוכן ממשי.
//4.טיפול בשגיאות(Error Handling)
//הקפנו הכל ב-try-catch. אם למשל המשתמש ינסה לשמור קובץ בתיקייה שאין לו הרשאה אליה, או בנתיב לא קיים, האפליקציה לא "תתרסק" אלא תדפיס הודעה ידידותית למשתמש.

var createRspCommand = new Command("create-rsp", "Create a response file for the bundle command");
createRspCommand.SetHandler(() =>
{
    try
    {
        // נאסוף את כל התשובות לתוך מחרוזת אחת שתהפוך לפקודה
        string commandOptions = "bundle ";

        // 1. שאלת שפות (חובה)
        Console.WriteLine("Enter list of languages (java, html, cs, py, js ) or 'all':");

        string languages = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(languages))
        {
            Console.WriteLine("Error: Languages are required! Please enter values:");
            languages = Console.ReadLine();
        }
        commandOptions += $"--language {languages} ";

        // 2. נתיב פלט
        Console.WriteLine("Enter output file path and name (e.g., 'myBundle.txt'):");
        string output = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(output))
        {
            Console.WriteLine("Error: Output path is required!");
            output = Console.ReadLine();
        }
        commandOptions += $"--output \"{output}\" ";

        // 3. האם להוסיף הערות מקור?
        Console.WriteLine("Would you like to include source paths as comments? (y/n):");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            commandOptions += "--note ";
        }

        // 4. מיון
        Console.WriteLine("Sort by 'name' (alphabetical) or 'type' (extension)? [default: name]:");
        string sort = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(sort))
        {
            if (sort != "name" && sort != "type")
            {
                Console.WriteLine("Error: sort must be either 'name' or 'type'.");
                return;
            }

            commandOptions += $"--sort {sort} ";
        }
        // 5. הסרת שורות ריקות
        Console.WriteLine("Remove empty lines? (y/n):");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            commandOptions += "--remove-empty-lines ";
        }

        // 6. שם יוצר
        Console.WriteLine("Enter author name (optional):");
        string author = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(author))
        {
            commandOptions += $"--author \"{author}\" ";
        }

        // יצירת קובץ ה-RSP
        string rspFileName = "options.rsp";
        File.WriteAllText(rspFileName, commandOptions);

        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($"Success! Response file created: {rspFileName}");
        Console.WriteLine($"cli @{rspFileName}");
        Console.WriteLine("--------------------------------------------------");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while creating the response file: {ex.Message}");
    }
});
//סבר על מה שקורה כאן:
//בדיקות תקינות(Validation): השתמשנו בלולאת while עבור שדות חובה (כמו שפה ופלט). אם המשתמש לוחץ Enter בלי לכתוב כלום, התוכנה לא תמשיך עד שהוא יזין ערך.
//תמיכה ברווחים: בנתיב הפלט ובשם היוצר הוספנו גרשיים (\") סביב הקלט. זה חשוב כי אם המשתמש יכתוב My Project.txt, ה-CLI עלול לחשוב שאלו שני פרמטרים שונים. הגרשיים שומרים עליהם כיחידה אחת.
//@fileName.rsp: הפורמט של קובץ תגובה ב-C# CLI הוא פשוט שורת הפקודה שכתובה בתוך קובץ. כשהמשתמש מריץ פקודה עם @, המערכת "שואבת" את התוכן מהקובץ ומריצה אותו כאילו הוקלד ידנית.

// 3. הגדרת ה-RootCommand
var rootCommand = new RootCommand("Root command for File Bundler CLI");

rootCommand.AddCommand(createRspCommand);
rootCommand.AddCommand(bundleCommand);
await rootCommand.InvokeAsync(args);
