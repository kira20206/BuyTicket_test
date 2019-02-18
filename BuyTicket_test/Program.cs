using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using HtmlAgilityPack;

namespace BuyTicket_test
{
    class Program
    {
        
        static string KeyWord = "特A2區";          //座位關鍵字
        static int TicketPiece = 2;                 //購買張數
        static IWebDriver driver;
        static string GoAreaUrl;                    //購票連結
        static bool BuyButtonLoss = false;
        static bool AreaKeywordLoss = false;        //座位關鍵字搜尋失敗
        static bool ErrorCheck = false;             //若錯誤，一直重新整理

        static void Main(string[] args)
        {


            //關閉Chrome彈出的訊息通知
            ChromeOptions options = new ChromeOptions();
            //options.AddArguments("headless");                     //無頭模式(無瀏覽器畫面)
            options.AddArguments("--disable-notifications");        //關閉Chrome內建的提示
            options.AddArguments("--ignore-certificate-errors");    //關閉ERROR:ssl_client_socket_impl.cc(1098)訊息

            driver = new ChromeDriver(options);      //將關閉通知的設定寫入到Chrome裡面
            //driver.Url = "https://tixcraft.com/activity/detail/19_MAROON5";
            driver.Url = "https://tixcraft.com/activity/detail/19_JOKERXUE";
            //driver.Url = "https://tixcraft.com/activity/detail/19_YJS";

            //driver.Manage().Window.Maximize();                //視窗最大化
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);  //隱性等待，主要是網頁的完整性，拿掉會錯誤

            //Cookie紀錄管理，將資料寫入cookie登入系統
            ICookieJar listcookie = driver.Manage().Cookies;

            #region 會員Cookie寫入，需自己填入在參數2那邊
            OpenQA.Selenium.Cookie newCookie0 = new OpenQA.Selenium.Cookie("_ga", "", "", DateTime.Now.AddDays(1));
            OpenQA.Selenium.Cookie newCookie1 = new OpenQA.Selenium.Cookie("_gid", "", "", DateTime.Now.AddDays(1));
            OpenQA.Selenium.Cookie newCookie2 = new OpenQA.Selenium.Cookie("CSRFTOKEN", "", "", DateTime.Now.AddDays(1));
            OpenQA.Selenium.Cookie newCookie3 = new OpenQA.Selenium.Cookie("_gat", "", "", DateTime.Now.AddDays(1));
            OpenQA.Selenium.Cookie newCookie4 = new OpenQA.Selenium.Cookie("SID", "", "", DateTime.Now.AddDays(1));
            OpenQA.Selenium.Cookie newCookie5 = new OpenQA.Selenium.Cookie("lang", "", "", DateTime.Now.AddDays(1));

            listcookie.AddCookie(newCookie0);
            listcookie.AddCookie(newCookie1);
            listcookie.AddCookie(newCookie2);
            listcookie.AddCookie(newCookie3);
            listcookie.AddCookie(newCookie4);
            listcookie.AddCookie(newCookie5);
            #endregion

            //關閉拓元的提示
            driver.FindElement(By.ClassName("closeNotice")).Click();

            driver.Navigate().Refresh();

            //滾動捲軸
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/5);");

            //點選立即購票
            ClickBuyTicketNow();

            //取得購票連結
            try
            {
                GoAreaUrl = driver.FindElement(By.XPath("//*[@id='gameList']/table/tbody/tr/td[4]/input")).GetAttribute("data-href");
                driver.Url = "https://tixcraft.com" + GoAreaUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine("取得購票連結錯誤，重新取得中...");
                ErrorCheck = true;

                while (ErrorCheck)
                {
                    try
                    {
                        GoAreaUrl = driver.FindElement(By.XPath("//*[@id='gameList']/table/tbody/tr/td[4]/input")).GetAttribute("data-href");
                        driver.Url = "https://tixcraft.com" + GoAreaUrl;
                        ErrorCheck = false;
                        break;
                    }
                    catch
                    {
                        Console.WriteLine("取得購票連結錯誤，重新取得中...");
                        driver.Navigate().Refresh();
                        ClickBuyTicketNow();
                    }
                }
            }


            //座位關鍵字點擊
            try
            {
                driver.FindElement(By.XPath("//a[contains(text(), '" + KeyWord + "')]")).Click();
            }
            catch (Exception ex)
            {
                Console.WriteLine("查詢座位關鍵字錯誤，重新取得中...");
                AreaKeywordLoss = true;

                if (AreaKeywordLoss)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (i == 0)
                            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/4);");
                        else if (i == 1)
                            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/2);");
                        else if (i == 2)
                            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/4*3);");
                        else
                            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

                        //關鍵字點擊
                        try
                        {
                            driver.FindElement(By.XPath("//*[contains(text(), '" + KeyWord + "')]")).Click();
                            AreaKeywordLoss = false;
                            break;
                        }
                        catch
                        {
                            Console.WriteLine("第" + (i + 1) + "次找不到關鍵字座位");
                            if (i == 4)
                            {
                                Console.WriteLine("找不到關鍵字座位，請重新設定關鍵字座位");
                            }
                        }
                    }
                }
            }

            //選擇張數
            DropDownList_SelectValue(By.ClassName("mobile-select"), TicketPiece.ToString());

            //同意會員服務條款
            driver.FindElement(By.XPath("//*[@id='TicketForm_agree']")).Click();

            //等待購買方式出現後繼續，只等300秒，不然會拋出錯誤訊息。等候結帳應該不會等太久
            new WebDriverWait(driver, TimeSpan.FromSeconds(300)).Until(ExpectedConditions.ElementExists((By.Id("PaymentForm_payment_id_36"))));

            //選擇信用卡結帳
            driver.FindElement(By.XPath("//*[@id='PaymentForm_payment_id_36']")).Click();

            //選擇iBon結帳




            ////利用HtmlAgilityPack解網頁程式碼
            //HtmlWeb web = new HtmlWeb();
            //var HtmlDoc = web.Load("https://tixcraft.com" + GoAreaUrl);
            //var AllAreaUl = HtmlDoc.DocumentNode.SelectSingleNode("//div[@class='zone area-list']").SelectNodes(".//ul");
        }

        //副程式

        //下拉式選單動作
        static void DropDownList_SelectValue(By element, string value)
        {
            // 尋找網頁元件
            IWebElement webElement = driver.FindElement(element);

            // 建立下拉選單物件
            SelectElement selectedElement = new SelectElement(webElement);

            // 改變下拉選單的項目
            selectedElement.SelectByValue(value);
        }

        //點開立即購票動作
        static void ClickBuyTicketNow()
        {
            try
            {
                //driver.FindElement(By.XPath("//*[@id='content']/div/div/ul/li[1]")).Click();
                driver.FindElement(By.XPath("//*[contains(text(), '立即購票')]")).Click();
            }
            catch (Exception ex)
            {
                driver.Navigate().Refresh();
                Console.WriteLine("無立即購票按鈕，重新整理中...");
                BuyButtonLoss = true;

                while (BuyButtonLoss)
                {
                    try
                    {
                        //driver.FindElement(By.XPath("//*[@id='content']/div/div/ul/li[1]")).Click();
                        driver.FindElement(By.XPath("//*[contains(text(), '立即購票')]")).Click();
                        BuyButtonLoss = false;
                        break;
                    }
                    catch
                    {
                        driver.Navigate().Refresh();
                        Console.WriteLine("無立即購票按鈕，重新整理中...");
                    }
                }
            }
        }
    }
}
