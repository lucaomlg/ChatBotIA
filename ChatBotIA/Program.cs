﻿using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;
using PuppeteerSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;

string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"; // Ajuste conforme necessário

// Configura o navegador com opções adicionais
var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = false, // Use headless: true para não exibir a UI
    ExecutablePath = chromePath,
    Args = new[]
    {
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-blink-features=AutomationControlled", // Oculta o aviso de controle
                "--remote-debugging-port=9222", // Permite depuração remota
                "--user-data-dir=C:\\Users\\lucas.santos\\AppData\\Local\\Google\\Chrome\\User Data\\Default" // Usar um perfil de usuário temporário
            }
});

// Abre uma nova página
var page = await browser.NewPageAsync();

// Executa scripts para modificar o navegador e ocultar a detecção
await page.EvaluateFunctionOnNewDocumentAsync(@"() => {
                Object.defineProperty(navigator, 'webdriver', { get: () => undefined });
                const newUA = navigator.userAgent.replace('HeadlessChrome', 'Chrome');
                Object.defineProperty(navigator, 'userAgent', { get: () => newUA });
                Object.defineProperty(navigator, 'languages', { get: () => ['en-US', 'en'] });
                window.addEventListener('mousemove', () => {});
                window.addEventListener('keypress', () => {});
        }");

// Navega até a página desejada
await page.GoToAsync("https://chat.openai.com/");

var a = "Olá, ChatGPT!";
int b = 0;
// Aguarda o campo de entrada de mensagem estar disponível
while (a != "para")
{
    try
    {
        await page.WaitForSelectorAsync("textarea");

        // Envia uma mensagem
        await page.TypeAsync("textarea", a);
        await page.Keyboard.PressAsync("Enter");

        // Aguarda a resposta e captura o texto
        //await page.WaitForSelectorAsync("w-full text-token-text-primary");
        Thread.Sleep(10000);
        var allParagraphs = await page.QuerySelectorAllAsync("p");
        // Pega o último elemento <p>
        var lastParagraph = allParagraphs[allParagraphs.Count()-2];

        // Obtém o texto do último elemento <p>
        var responseText = await lastParagraph.EvaluateFunctionAsync<string>("el => el.innerText");
        //var response = await page.QuerySelectorAsync("p");
        //var responseText = await response.EvaluateFunctionAsync<string>("el => el.innerText");

        Console.WriteLine("Resposta do ChatGPT: " + responseText);

        a = Console.ReadLine();
        b++;
    }
    catch (Exception ex) 
    {
        a = ex.Message;
    }
}
// Fecha o navegador
await browser.CloseAsync();