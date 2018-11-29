using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RainBorg
{
    class Config
    {
        public static async Task Load()
        {
            // Check if config file exists and create it if it doesn't
            if (File.Exists(RainBorg.configFile))
            {
                // Load values
                JObject Config = JObject.Parse(File.ReadAllText(RainBorg.configFile));
                RainBorg.currencyName = (string)Config["currencyName"];
                RainBorg.decimalPlaces = (int)Config["decimalPlaces"];
                RainBorg.databaseFile = (string)Config["databaseFile"];
                RainBorg.balanceUrl = (string)Config["balanceUrl"];
                RainBorg.botAddress = (string)Config["botAddress"];
                RainBorg.botPaymentId = (string)Config["botPaymentId"];
                RainBorg.entranceMessage = (string)Config["entranceMessage"];
                RainBorg.successReact = (string)Config["successReact"];
                RainBorg.tipFee = (decimal)Config["tipFee"];
                RainBorg.tipMin = (decimal)Config["tipMin"];
                RainBorg.tipMax = (decimal)Config["tipMax"];
                RainBorg.userMin = (int)Config["userMin"];
                RainBorg.userMax = (int)Config["userMax"];
                RainBorg.waitMin = (int)Config["waitMin"];
                RainBorg.waitMax = (int)Config["waitMax"];
                RainBorg.megaTipAmount = (decimal)Config["megaTipAmount"];
                RainBorg.megaTipChance = (double)Config["megaTipChance"];
                RainBorg.accountAge = (int)Config["accountAge"];
                RainBorg.timeoutPeriod = (int)Config["timeoutPeriod"];
                RainBorg.flushPools = (bool)Config["flushPools"];
                RainBorg.developerDonations = (bool)Config["developerDonations"];
                RainBorg.logLevel = (int)Config["logLevel"];
                RainBorg.logFile = (string)Config["logFile"];
                RainBorg.botToken = (string)Config["botToken"];
                RainBorg.botPrefix = (string)Config["botPrefix"];
                RainBorg.tipPrefix = (string)Config["tipPrefix"];
                RainBorg.spamWarning = (string)Config["spamWarning"];
                RainBorg.localTipBot = (bool)Config["localTipBot"];
                RainBorg.tipBotId = (ulong)Config["tipBotId"];
                RainBorg.tipBotDatabaseFile = (string)Config["tipBotDatabaseFile"];
                RainBorg.ChannelWeight = Config["channelWeight"].ToObject<List<ulong>>();
                RainBorg.StatusChannel = Config["statusChannel"].ToObject<List<ulong>>();
                RainBorg.wordFilter = Config["wordFilter"].ToObject<List<string>>();
                RainBorg.requiredRoles = Config["requiredRoles"].ToObject<List<string>>();
                RainBorg.ignoredNicknames = Config["ignoredNicknames"].ToObject<List<string>>();
                RainBorg.statusImages = Config["statusImages"].ToObject<List<string>>();
                RainBorg.donationImages = Config["donationImages"].ToObject<List<string>>();
                foreach (ulong Id in RainBorg.ChannelWeight)
                    if (!RainBorg.UserPools.ContainsKey(Id))
                        RainBorg.UserPools.Add(Id, new LimitedList<ulong>());
            }
            else await Save();
        }

        public static Task Save()
        {
            // Store values
            JObject Config = new JObject
            {
                ["currencyName"] = RainBorg.currencyName,
                ["decimalPlaces"] = RainBorg.decimalPlaces,
                ["databaseFile"] = RainBorg.databaseFile,
                ["balanceUrl"] = RainBorg.balanceUrl,
                ["botAddress"] = RainBorg.botAddress,
                ["botPaymentId"] = RainBorg.botPaymentId,
                ["entranceMessage"] = RainBorg.entranceMessage,
                ["successReact"] = RainBorg.successReact,
                ["tipFee"] = RainBorg.tipFee,
                ["tipMin"] = RainBorg.tipMin,
                ["tipMax"] = RainBorg.tipMax,
                ["userMin"] = RainBorg.userMin,
                ["userMax"] = RainBorg.userMax,
                ["waitMin"] = RainBorg.waitMin,
                ["waitMax"] = RainBorg.waitMax,
                ["megaTipAmount"] = RainBorg.megaTipAmount,
                ["megaTipChance"] = RainBorg.megaTipChance,
                ["accountAge"] = RainBorg.accountAge,
                ["timeoutPeriod"] = RainBorg.timeoutPeriod,
                ["flushPools"] = RainBorg.flushPools,
                ["developerDonations"] = RainBorg.developerDonations,
                ["logLevel"] = RainBorg.logLevel,
                ["logFile"] = RainBorg.logFile,
                ["botToken"] = RainBorg.botToken,
                ["botPrefix"] = RainBorg.botPrefix,
                ["tipPrefix"] = RainBorg.tipPrefix,
                ["spamWarning"] = RainBorg.spamWarning,
                ["localTipBot"] = RainBorg.localTipBot,
                ["tipBotDatabaseFile"] = RainBorg.tipBotDatabaseFile,
                ["tipBotId"] = RainBorg.tipBotId,
                ["channelWeight"] = JToken.FromObject(RainBorg.ChannelWeight),
                ["statusChannel"] = JToken.FromObject(RainBorg.StatusChannel),
                ["wordFilter"] = JToken.FromObject(RainBorg.wordFilter),
                ["requiredRoles"] = JToken.FromObject(RainBorg.requiredRoles),
                ["ignoredNicknames"] = JToken.FromObject(RainBorg.ignoredNicknames),
                ["statusImages"] = JToken.FromObject(RainBorg.statusImages),
                ["donationImages"] = JToken.FromObject(RainBorg.donationImages)
            };

            // Flush to file
            File.WriteAllText(RainBorg.configFile, Config.ToString());

            // Completed
            return Task.CompletedTask;
        }
    }
}
