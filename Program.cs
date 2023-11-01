using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        await ProcessInput();
    }

    private static async Task ProcessInput()
    {
        // ici j'utilise la Clé d'API OpenAI que vous nous avez donné
        string apiKey = "";

        // URL de l'API Text-DaVinci-003
        string endpoint = "https://api.openai.com/v1/engines/text-davinci-003/completions";

        do
        {
            // j'affiche les options disponibles pour l'utilisateur
            Console.WriteLine("Options : ");
            Console.WriteLine("c : Corriger des fautes d'orthographe.");
            Console.WriteLine("an : Traduire en anglais.");
            Console.WriteLine("e : Traduire en espagnol.");
            Console.WriteLine("all : Traduire en allemand.");
            Console.WriteLine("create : Créer une application React.");
            Console.WriteLine("q : Quitter.");
            Console.Write("Entrez votre choix : ");

            // ici je récupère le choix de l'utilisateur
            var choice = Console.ReadLine()?.ToLower() ?? string.Empty;

            switch (choice)
            {
                case "q":
                    // Quitte le programme si on saisi "q"
                    return;

                case "c":
                case "an":
                case "e":
                case "all":
                    // ici je gère la traduction ou la correction en fonction du choix
                    await HandleTranslationOrCorrection(choice, apiKey, endpoint);
                    break;

                case "create":
                    // ici je Crée une application React
                    CreateReactApp();
                    break;

                default:
                    // aici j' ffiche un message d'erreur en cas de choix invalide
                    Console.WriteLine("Option non valide. Veuillez entrer 'c', 'an', 'e', 'all', 'create' ou 'q'.");
                    break;
            }
        } while (true);
    }

    private static async Task HandleTranslationOrCorrection(string choice, string apiKey, string endpoint)
    {
        // ensuite je détermine l'action en fonction du choix de l'utilisateur
        string action = choice switch
        {
            "c" => "corriger",
            "an" => "traduire en anglais",
            "e" => "traduire en espagnol",
            "all" => "traduire en allemand",
            _ => throw new InvalidOperationException("Choix non valide.")
        };

        // ici je demande à l'utilisateur d'entrer le texte à traiter
        Console.Write($"Entrez le texte à {action} : ");
        var inputText = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(inputText))
        {
            // je vérifie si l'entrée est vide et affiche un message d'erreur
            Console.WriteLine("L'entrée ne peut pas être vide.");
            return;
        }

        // je crée le texte d'instruction pour l'API en fonction du choix , je l'ai adapté pour avoir ce que je veux uniquement
        string prompt = choice switch
        {
            "c" => $"Corrigez cette phrase sans compléter la phrase : {inputText}",
            "an" => $"Traduisez cette phrase ou le groupe de mot exactement en anglais : {inputText}",
            "e" => $"Traduisez cette phrase ou le groupe de mot exactement en espagnol : {inputText}",
            "all" => $"Traduisez cette phrase ou le groupe de mot exactement en allemand : {inputText}",
            _ => throw new InvalidOperationException("Choix non valide.")
        };

        using (HttpClient httpClient = new HttpClient())
        {
            // ici je Configure l'en-tête d'autorisation avec la clé API
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var request = new
            {
                prompt = prompt,
                max_tokens = 150
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                // ici je gere le Traitement de la réponse de l'API
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var parsedResponse = JsonSerializer.Deserialize<OpenAIChatResponse>(jsonResponse);
                if (parsedResponse?.choices != null && parsedResponse.choices.Length > 0)
                {
                    // et Affichage de la réponse et écriture dans un fichier
                    Console.WriteLine(parsedResponse.choices[0].text?.Trim() ?? string.Empty);
                    File.WriteAllText("C:\\Users\\pc\\Desktop\\CLI\\SpellingCLI\\result.txt", parsedResponse.choices[0].text?.Trim() ?? string.Empty);
                }
            }
            else
            {
                // En cas d'erreur, j'affiche le code d'erreur et les détails
                Console.WriteLine($"Erreur : {response.StatusCode}. Détails : {await response.Content.ReadAsStringAsync()}");
            }
        }

        // je demande à l'utilisateur s'il souhaite exécuter une autre commande
        Console.Write("Voulez-vous exécuter une autre commande? (y/n) : ");
        var continueResponse = Console.ReadLine()?.ToLower();
        if (continueResponse == "n" || continueResponse == "no")
        {
            // et je Ferme l'application
            Environment.Exit(0);
        }
    }

    private static void CreateReactApp()
    {
        Console.Write("Entrez le nom de votre nouvelle application React : ");
        var appName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(appName))
        {
            // je vérifie si le nom est vide et affiche un message d'erreur
            Console.WriteLine("Le nom de l'application ne peut pas être vide.");
            return;
        }

        var desktopPath = "C:\\Users\\pc\\Desktop";
        var fullPath = Path.Combine(desktopPath, appName);

        if (Directory.Exists(fullPath))
        {
            // je vérifie aussi si le dossier existe déjà sur le bureau
            Console.WriteLine($"Un dossier nommé {appName} existe déjà sur le bureau.");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            WorkingDirectory = desktopPath,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process process = new Process { StartInfo = startInfo };
        process.Start();

        using (StreamWriter sw = process.StandardInput)
        {
            if (sw.BaseStream.CanWrite)
            {
                // j'exécute les commandes pour créer l'application React
                sw.WriteLine($"npx create-react-app {appName}");
                sw.WriteLine($"cd {appName}");
                sw.WriteLine("npm install"); // Si vous avez d'autres dépendances à installer, ajoute les ici 
                sw.WriteLine("npm install"); // Si vous avez d'autres dépendances à installer, ajoutez-les ici pour tester 
            }
        }

        process.WaitForExit();
        Console.WriteLine($"L'application React {appName} a été créée sur le bureau et les dépendances nécessaires ont été installées.");
    }

    public class OpenAIChatResponse
    {
        public Choice?[]? choices { get; set; }
    }

    public class Choice
    {
        public string? text { get; set; }
    }
}
