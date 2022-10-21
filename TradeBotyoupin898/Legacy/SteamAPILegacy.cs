using SteamAuth;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;

using TradeBotyoupin898;

namespace Legacy
{
    internal class SteamAPILegacy
    {
        private SteamGuardAccount[] allAccounts;
        private SteamGuardAccount currentAccount;
        private Manifest manifest;

        public SteamAPILegacy()
        {
            manifest = Manifest.GetManifest();
            allAccounts = manifest.GetAllAccounts();
            currentAccount = allAccounts.Single();
        }

        public Confirmation[] GetConfirmation()
        {
            refreshSession();
            return currentAccount.FetchConfirmations();
        }

        public bool AcceptConfirmation(Confirmation conf)
        {
            return currentAccount.AcceptConfirmation(conf);
        }

        public void AcceptOffer(OrderDataLegacy order)
        {
            var postData = new NameValueCollection
            {
                { "partner", order.OtherSteamId.ToString() },
                { "serverid", "1" },
                { "sessionid", currentAccount.Session.SessionID },
                { "tradeofferid", order.SteamOfferId },
                { "captcha", string.Empty }
            };

            refreshSession();
            CookieContainer cookies = new CookieContainer();
            cookies.Add(new Cookie("sessionid", currentAccount.Session.SessionID, "/", ".steamcommunity.com"));
            cookies.Add(new Cookie("steamLoginSecure", currentAccount.Session.SteamLoginSecure, "/", ".steamcommunity.com")
            {
                HttpOnly = true,
                Secure = true
            });

            SteamWeb.Request($"{APIEndpoints.COMMUNITY_BASE}/tradeoffer/{order.SteamOfferId}/accept", "POST", data: postData, cookies: cookies, referer: $"{APIEndpoints.COMMUNITY_BASE}/tradeoffer/{order.SteamOfferId}");
        }

        private void refreshSession()
        {
            if (currentAccount.RefreshSession())
            {
                manifest.SaveAccount(currentAccount);
            }
            else
            {
                Console.WriteLine("Steam登录过期，请手动刷新");
            }
        }
    }
}
