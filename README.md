# File Bundler CLI 🚀

[cite_start]A powerful Command Line Interface (CLI) tool built with C# and .NET 8  to bundle multiple source code files into a single, organized text file. Ideal for sharing codebases, backups, or providing context to AI models.

## Features ✨

* **Multi-Language Support:** Bundle files by extension (Java, C#, Python, JS, HTML, etc.) or use the `all` keyword.
* **Smart Filtering:** Automatically excludes build and version control directories like `bin`, `obj`, `debug`, `.vs`, and `.git`.
* **Custom Sorting:** Sort files alphabetically by name or by file type (extension).
* **Code Cleaning:** Option to remove empty lines from the source code.
* **Metadata:** Add the author's name and source file paths as comments within the bundle.
* **Response Files (RSP):** Interactive wizard to create `.rsp` files for easy command reuse.


## 🛠️ How to Run and Install

To use this tool on your machine, follow these steps:

### 1. Prerequisites
* Ensure you have the **.NET 8.0 SDK** installed[cite: 1]. You can download it from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0).

### 2. Running from Source (Development Mode)
If you want to run the code directly without installation:
1.  Open your terminal in the `CLI/` folder.
2.  Use the `dotnet run` command followed by `--` and your arguments:
    ```bash
    dotnet run -- bundle --language all --output bundle.txt
    ```

### 3. Installing as a Global Tool (Recommended)
To use the command `cli` from **anywhere** on your computer:
1.  **Build the package:**
    ```bash
    dotnet pack
    ```
2.  **Install it locally:**
    ```bash
    dotnet tool install --add-source ./nupkg --global CLI
    ```
    *(Note: After installation, you can simply type `cli bundle ...` in any folder).*

---

## Option,           Alias,   Description,                                                           Required
--language,           -l,      "List of programming languages (e.g., cs, py, js) or all",               Yes
--output,             -o,      File path and name for the bundled output file,                          Yes
--note,               -n,      Include the source file path as a header comment for each file,          No
--sort,               -s,      Sort files by name (alphabetical) or type (extension). Default: name,    No
--remove-empty-lines, -r,      Strip all blank lines from the source code,                              No
--author,             -a,      Add the name of the file creator at the top of the bundle,               No

### 💻 Usage Examples

  cli bundle -l cs py -o ./myCode.txt -n -s type -a "Your Name" -r

### The `bundle` Command
The `bundle` command collects files based on your preferences:

  cli bundle -language cs py --output ./myCode.txt --note --sort type --author "Your Name"

### The create-rsp Command
Generate a configuration file (Response File) interactively:


### cli create-rsp
To run the generated file:

     cli @options.rsp
### 🏗️ Technologies Used

.NET 8.0 SDK 

System.CommandLine (Library for argument parsing) 

LINQ (For efficient file filtering and sorting)
