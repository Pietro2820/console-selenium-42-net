using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace ConsoleSelenium.Tests;

[TestClass]
public class BuscaDesafioTest
{
    private const string UrlBase = "https://duckduckgo.com";
    private const string SeletorSugestoes =
        ".search-suggestions-list li, .sbox-suggestions li, [role='option']";
    private const string SeletorPrimeiroResultado = "a[data-testid='result-title-a']";

    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--lang=en-US");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
    }

    [TestCleanup]
    public void Teardown()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public void Exercicio1_PesquisaBasica_ValidaTitulo()
    {
        const string termo = "Selenium WebDriver C#";

        var campo = AbrirHomeELocalizarCampoDeBusca();
        campo.SendKeys(termo);
        campo.SendKeys(Keys.Enter);

        _wait.Until(d => d.Url.Contains("q="));

        Assert.IsTrue(
            ConsultaDaUrl().Contains(termo, StringComparison.OrdinalIgnoreCase),
            $"A URL de resultados deveria conter '{termo}', mas foi: {_driver.Url}");
    }

    [TestMethod]
    public void Exercicio2_Autocompletar_SelecionaTerceiraSugestao()
    {
        const string termo = "C# unit testing";

        var campo = AbrirHomeELocalizarCampoDeBusca();
        campo.SendKeys(termo);

        var sugestoes = _wait.Until(d =>
        {
            var itens = d.FindElements(By.CssSelector(SeletorSugestoes))
                         .Where(e => e.Displayed)
                         .ToList();
            return itens.Count >= 3 ? itens : null;
        })!;

        var terceira = sugestoes[2];
        var textoSugestao = NormalizarEspacos(terceira.Text);
        terceira.Click();

        _wait.Until(d => d.Url.Contains("q="));

        Assert.IsTrue(
            ConsultaDaUrl().Contains(textoSugestao, StringComparison.OrdinalIgnoreCase),
            $"A busca deveria refletir a sugestao '{textoSugestao}', mas a URL foi: {_driver.Url}");
    }

    [TestMethod]
    public void Exercicio3_NavegacaoHistorico_VoltaParaResultados()
    {
        const string termo = "Automated Testing";

        var campo = AbrirHomeELocalizarCampoDeBusca();
        campo.SendKeys(termo);
        campo.SendKeys(Keys.Enter);

        _wait.Until(d => d.Url.Contains("q="));
        var urlResultados = _driver.Url;

        var primeiroLink = _wait.Until(d =>
        {
            var link = d.FindElements(By.CssSelector(SeletorPrimeiroResultado))
                        .FirstOrDefault(e => e.Displayed);
            return link;
        })!;
        primeiroLink.Click();

        _wait.Until(d => !d.Url.Contains("duckduckgo.com"));
        Assert.IsFalse(_driver.Url.Contains("duckduckgo.com"),
            $"Deveria ter saido do DuckDuckGo, mas a URL atual e: {_driver.Url}");

        _driver.Navigate().Back();

        _wait.Until(d => d.Url.Contains("duckduckgo.com"));
        Assert.IsTrue(_driver.Url.Contains("duckduckgo.com"));
        Assert.IsTrue(_driver.Url.Contains("q="),
            $"A pagina de resultados nao foi restaurada. URL atual: {_driver.Url}");
        Assert.AreEqual(urlResultados, _driver.Url);
    }

    private IWebElement AbrirHomeELocalizarCampoDeBusca()
    {
        _driver.Navigate().GoToUrl(UrlBase);
        return _wait.Until(d => d.FindElement(By.Name("q")));
    }

    private string ConsultaDaUrl()
    {
        var url = _driver.Url.Replace('+', ' ');
        return NormalizarEspacos(Uri.UnescapeDataString(url));
    }

    private static string NormalizarEspacos(string texto) =>
        string.Join(' ', texto.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
