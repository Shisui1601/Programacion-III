using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace Consoleprueba
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configuración del reporte
            string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "TestReport.html");
            var htmlReporter = new ExtentSparkReporter(reportPath);
            var extent = new ExtentReports();
            extent.AttachReporter(htmlReporter);

            // Crear un caso de prueba en el reporte
            var testSuccess = extent.CreateTest("Prueba de Login - Caso de Éxito");
            var testFailure = extent.CreateTest("Prueba de Login - Caso de Fracaso");

            IWebDriver driver = null;

            try
            {
                // Configuración del Edge WebDriver
                var options = new EdgeOptions();
                driver = new EdgeDriver(options);

                #region Caso de Éxito
                // Prueba de inicio de sesión exitoso
                testSuccess.Log(AventStack.ExtentReports.Status.Info, "Navegando a la página de inicio de sesión");
                driver.Navigate().GoToUrl("file:///C:/Users/Santiago%20Aguero/Desktop/pagina%20web/login.html");
                driver.Manage().Window.Maximize();


               

                // Automatizar el formulario de inicio de sesión con credenciales correctas
                driver.FindElement(By.Id("username")).SendKeys("admin");
                driver.FindElement(By.Id("password")).SendKeys("12345");
                string screenshotPath = CaptureScreenshot(driver, "LoginPage");
                testSuccess.AddScreenCaptureFromPath(screenshotPath, "Página de inicio de sesión");
                driver.FindElement(By.ClassName("submit")).Click();

                // Manejar la alerta de inicio de sesión exitoso
                HandleAlert(driver, testSuccess);

                screenshotPath = CaptureScreenshot(driver, "LoggedInPage");
                testSuccess.AddScreenCaptureFromPath(screenshotPath, "Página después del inicio de sesión");


                // Esperar unos segundos antes de continuar
                System.Threading.Thread.Sleep(3000); // Espera 3 segundos

                // Regresar a la página de login
                driver.Navigate().GoToUrl("file:///C:/Users/Santiago%20Aguero/Desktop/pagina%20web/login.html");

                #endregion

                #region Caso de Fracaso
                // Prueba de inicio de sesión fallido
                testFailure.Log(AventStack.ExtentReports.Status.Info, "Navegando a la página de inicio de sesión");
                driver.Navigate().GoToUrl("file:///C:/Users/Santiago%20Aguero/Desktop/pagina%20web/login.html");
                driver.Manage().Window.Maximize();

                screenshotPath = CaptureScreenshot(driver, "LoginPage_Failure");
                testFailure.AddScreenCaptureFromPath(screenshotPath, "Página de inicio de sesión");

                // Automatizar el formulario de inicio de sesión con credenciales incorrectas
                driver.FindElement(By.Id("username")).SendKeys("usuarioIncorrecto");
                driver.FindElement(By.Id("password")).SendKeys("claveIncorrecta");
                driver.FindElement(By.ClassName("submit")).Click();

                // Manejar la alerta de error
                HandleAlert(driver, testFailure);
                screenshotPath = CaptureScreenshot(driver, "FailedLoginAttempt");
                testFailure.AddScreenCaptureFromPath(screenshotPath, "Página después del intento de inicio de sesión");
                System.Threading.Thread.Sleep(3000);
                #endregion
            }
            catch (Exception ex)
            {
                // Si ocurre un error general
                testFailure.Fail("Error durante la prueba: " + ex.Message);
            }
            finally
            {
                // Cerrar el navegador
                driver?.Quit();
                testSuccess.Log(AventStack.ExtentReports.Status.Info, "Navegador cerrado");
                testFailure.Log(AventStack.ExtentReports.Status.Info, "Navegador cerrado");

                // Generar el reporte
                extent.Flush();
            }
        }

        // Método para capturar capturas de pantalla
        static string CaptureScreenshot(IWebDriver driver, string screenshotName)
        {
            try
            {
                string screenshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                Directory.CreateDirectory(screenshotsDir);

                string screenshotPath = Path.Combine(screenshotsDir, $"{screenshotName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(screenshotPath);

                return screenshotPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al capturar la pantalla: " + ex.Message);
                return null;
            }
        }

        // Método para manejar alertas
        static void HandleAlert(IWebDriver driver, ExtentTest test)
        {
            try
            {
                // Cambiar el foco a la alerta
                IAlert alert = driver.SwitchTo().Alert();
                Console.WriteLine("Alerta detectada: " + alert.Text);

                // Si es un inicio de sesión exitoso
                if (alert.Text.Contains("¡Inicio de sesión exitoso!"))
                {
                    test.Log(AventStack.ExtentReports.Status.Pass, "Inicio de sesión exitoso: " + alert.Text);
                }
                else if (alert.Text.Contains("Usuario o contraseña incorrectos"))
                {
                    test.Log(AventStack.ExtentReports.Status.Pass, "Inicio de sesión fallido: " + alert.Text);
                }

                alert.Accept();  // Aceptar la alerta
                test.Log(AventStack.ExtentReports.Status.Info, "Alerta manejada: " + alert.Text);
            }
            catch (NoAlertPresentException)
            {
                // Si no hay alerta, continuar sin hacer nada
                Console.WriteLine("No se encontró ninguna alerta.");
                test.Log(AventStack.ExtentReports.Status.Info, "No se encontró ninguna alerta.");
            }
        }
    }
}
