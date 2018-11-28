# RainBorgCore
RainBorg is a bot that takes in community donations and spreads them out over time to active users on Discord.

**This bot requires [TrtlBotSharp](https://github.com/turtlecoin/trtl-bot-csharp) to function.**

# Rundown of Functionality
This bot will keep a list of active users on a given set of Discord channels, and split a given amount between those users after a given period of time, with a given chance to tip all channels every so often, and users will be timed out after a given period of time. Confusing? Let me break that down.

Say you have two channels that you want to tip on, and for simplicity sake, say they are channel id 1 and channel id 2. Say you want one of these channels to be tipped twice as often as the other - you would want to set channel 1 to have a weight of one, and channel two to have a weight of two - therefore, channel 2 is twice as likely to be tipped on as channel 1.

When setting up the bot, you are able to supply a tip minimum and a tip maximum. When tipping, the bot will check that its balance falls inside of, or above this given range. If the balance is higher than this amount, it will tip the maximum tip amount - this is to prevent it from tipping out its full balance to a select number of users. The tip minimum is the lowest amount it will tip, any lower and it will wait until its balance is at least this amount. As an example, imagine a bot with a tip minimum of 10 coins, and a tip maximum of 20 coins. If this hypothetical bot had 5 coins in its balance, it would not tip and would wait until it has at least 10 coins, then it would split 10 coins between all active users. If this bot has 15 coins, it would split those 15 coins between those users. If this bot had 30 coins however, since the tip maximum is 20, it would split 20 coins between those users, with 10 being left in its balance for the next tip.

The next setting of note would be the wait minimum and wait maximum. These values specify the range of time (in seconds) that the bot will wait between trying to tip. For example, if the wait minimum was set to 300 seconds (5 minutes) and the wait maximum was set to 600 seconds (10 minutes), it would randomize a time between these two values and wait that long before tipping users, so it will be a random time between 5 and 10 minutes between tips.

Along with wait time, other time-specific values the bot uses are the user timeout period and account age. The user timeout time is how long (in seconds) the bot will wait since their last message to remove them from the next tip on that channel. For example, if there were two channels the bot was tipping on (channel 1 and channel 2), and a user was chatting on both, with user timeout set to 5 minutes, if they were active on channel 1 within the last 5 minutes they will be on that channel's active user list, but if their last message on channel 2 was 8 minutes ago, they will have been timed out, and removed from channel 2's active user list. Account age on the other hand is measured in hours, and is the amount of time a user's Discord account must have existed for before being eligible for tips. This is to prevent people from making a lot of new accounts to get more coins from the bot.

Another setting of note is the mega tip percentage. This is how frequent a "megatip" naturally occurs. A megatip tips all active users on all tippable channels a specified amount. For example, say the megatip percentage is at 1%, this means that 1% of all tips the bot does will be megatips. Imagine the megatip amount is set to 1000, and the bot is tipping on two channels (channel 1 and channel 2). Imagine there are 6 active users on channel 1 and 4 active on channel 2. If a megatip is called, then each active user on both channels will receive 100 coins each, as 6 + 4 is a total of 10 active users. Of note though is that if a user is active on channel 1 AND channel 2, then they will be tipped on BOTH channels, as they are seen as separate users on each channel - this is to encourage spreading out user activity to multiple channels.

An additonal setting of note is the status channel list. These channels are optional and simply are where the bot will say when it tips, how many users were tipped, how much was tipped, and how much each user received.

Lastly, whenever a tip is sent out, all relevant information (amount, date and time, recipient ID) are all logged into a stats database for future reference.

# Setup and Configuration
To begin setting up the RainBorg on your discord server, you must already have permission to add bots to your server. You must also already have a TrtlBotSharp tip bot running on your server.

Next, head to [Discord's developer page](https://discordapp.com/developers/applications/) and create an application. Name it something unique, then click Bots on the left side. After that, click Add Bot, change the username to whatever you'd like your bot to be called. Click to reveal the bot token, and keep this value on hand. Do NOT share this token with anyone.

From here, you will want to compile the bot, then create a `Config.conf` file in its running directory and edit it to suit, namely setting the botToken to the value you copied in the previous step. (Optionally, you can run the bot once without the config file, it will crash, but it will make the file for you, which you can then edit.) See [Config Breakdown](#Config-Breakdown) below to see what each config value means.

Once the bot can be run without crashing, you'll want to enter `addoperator 0` in the console window, replacing `0` with your user ID. (To get this ID, enable developer mode in your discord settings then right click your icon in Discord and click Copy ID.) This will add you as an operator, and you can then use operator commands.

From there, you must register a wallet with the tip bot using the `registerwallet` command. You can optionally also run a `redirecttips` function to have any funds tipped to directly to the RainBorg automatically go to its balance. Both of these functions must be run on a channel that both bots exist on, as Discord bots are unable to DM each other. To do this, go to a channel that both bots can see and type `$say .registerwallet address`, replacing the bot prefixes with the proper symbol, and address with your wallet's address. To redirect tips back to your bot's tip fund, you would use `$say .redirecttips`.

After that is all set up, you can start using operator commands to configure your bot further, see the documentation in the [RainBorg Wiki](https://github.com/turtlecoin/RainBorgCore/wiki/Operator-Commands).

# Config Breakdown

**`"currencyName": "TRTL",`**

Your currency's ticker symbol/abbreviation. In this case, the bot is tipping TurtleCoin, so it uses the TRTL ticker.

**`"decimalPlaces": 2,`**

How many decimal places your coin has. In this case, TurtleCoin has two decimal places.

**`"databaseFile": "Data.DB",`**

The SQLite database file that contains all user information and settings, as well as tip stats.

**`"balanceUrl": "http://trtlbotapi.krruzic.xyz/balance?pid=bca975edfe710a64337beb1685f32ab900989aa9767946efd8537f09db594bbd",`**

The URL to a tip bot endpoint that lets the bot know its balance. This is required to be set up on the tip bot's host machine, unless both bots are being hosted on the same server. (See `localTipBot` setting below.)

**`"botAddress": "TRTLv12WtKJAzTNtxSkbcXf7mjeVApSqRYACtoJE2X52UBSce7qGAQ1JQgG3MmArnZSbkJXKqBXiPX2Mno7xD4tqD3p8SySoBc5",`**

The tip bot's deposit address.

**`"botPaymentId": "bca975edfe710a64337beb1685f32ab900989aa9767946efd8537f09db594bbd",`**

Your bot's registered payment ID with the tip bot.

**`"successReact": "kthx",`**

Replace this with the reaction you want your bot to give when a command is successful. Optional.

**`"tipFee": 0.1,`**

Yur network's transaction fee. In this case, TurtleCoin's transaction fee is 0.1 TRTL.

**`"tipMin": 10.0,`**

The minimum amount the bot's balance must be at before it will tip. In this case, the bot's balance must be at least 10 TRTL.

**`"tipMax": 50.0,`**

The maximum amount a tip can be. In this case, the bot will tip a maximum of 10 TRTL.

**`"userMin": 3,`**

The minimum amount of users that must be active on a channel before it's eligible to be tipped on. In this case, a channel must have at least 3 active users to be tipped on.

**`"userMax": 10,`**

The maximum amount of users the bot will tip at once. In this case, it will only tip the last 10 active users on a channel.

**`"waitMin": 60,`**

The minimum amount of time (in seconds) that the bot will wait before tipping again. In this case, it will wait a minimum of 1 minute.

**`"waitMax": 600,`**

The maximum amount of time (in seconds) that the bot will wait before tipping again. In this case, it will wait a maximum of 10 minutes.

**`"megaTipAmount": 100.0,`**

When a megatip is called naturally (not through a command), it will split this amount between all active users on all channels. In this case, it would split 100 TRTL between these users.

**`"megaTipChance": 1.0,`**
How often a megatip will naturally occur. In this case, 1% of all tips the bot sends out will be megatips.

**`"accountAge": 24,`**

This minimum amount of time (in hours) a Discord account has to have existed for to be eligible for tipping. In this case, an account must have been created at least 24 hours ago to be eligible to receive tips.

**`"timeoutPeriod": 300,`**

How long to wait (in seconds) to remove a user from an active user list since their last message. In this case, if a user's last message was over 5 minutes ago, they will be removed from the active user list.

**`"flushPools": false,`**

Whether or not to clear the active user list after each tip. Only really matters if user timeout is set too long or a server is very busy.

**`"developerDonations": true,`**

If this is enabled, and I am on the same server, I will be included in all regular tip cycles as a donation for creating this bot. This is optional, but I very much appreciate it. If I am not on the server, this setting is ignored.

**`"logLevel": 2,`**

How much information is logged to the console/log file while running. 2 is usually adequate.

**`"logFile": "log.txt",`**

If this is not blank, all console information will be logged to a text file as well as being written to the console. The text file will be created if it does not exist.

**`"botToken": "12345",`**

Your bot's Discord token.

**`"botPrefix": "$",`**

The prefix before commands for this bot. In this case, commands written in Discord will have to start with `$`.

**`"tipPrefix": ".",`**

This is the prefix your tip bot uses. In this case, it will tip using `.tip`.


**`"spamWarning": "You've been issued a spam warning, this means you won't be included in my next tip. Try to be a better turtle, okay? ;) Consider reading up on how to be a good turtle:\nhttps://medium.com/@turtlecoin/how-to-be-a-good-turtle-20a427028a18",`**

What message is sent to users when you `$warn` them.

**`"entranceMessage": "",`**

If this is not blank, whenever the bot is launched or comes online, this message will be displayed on all tipping channels.

**`"localTipBot": false,`**

If this is set to true, it means you are hosting the tip bot on the same machine as the RainBorg, and it will read balance directly from the tip bot's database file, so an endpoint is not needed, and the `balanceUrl` setting is ignored.

**`"tipBotDatabaseFile": "users.db",`**

If the `localTipBot` setting is set to `true`, the balance will be read from this database file. This file must match the database file your tip bot is using, and is separate from the `databaseFile` setting.

**`"tipBotId": 413887034864697364,`**

This is the user ID of your tip bot. This is used to pause the bot if the tip bot goes offline for any reason. If this is set to 0, no checks are made.

**```"channelWeight": [
    1,
    2,
    2
  ],```**
  
List of channel IDs that the bot will tip on. Weight is determined by how many time an ID is in this list. In this case, channel ID 1 as a weight of one, and channel ID 2 has a weight of two.


**```"statusChannel": [
    0
  ],```**
  
This is a list of channels that status messages will be posted on. In this case, channel ID 0 will receive a status message any time a tip is sent.

**```"wordFilter": [
    "binance",
    "kucoin",
    "tradeogre"
  ],```**

Any messages containing words from this list will be ignored when checking for user activity.

**```"requiredRoles": [
    "turtle",
    "snapper",
    "terrapin",
    "hawksbill",
    "loggerhead",
    "leatherback",
    "tortoise",
    "archelon"
  ],```**

A user must have one of these roles when posting to be eligible to receive tips. If this list is empty, no role check is made.

**```"ignoredNicknames": [
    "(exiled)"
  ],```**

This list contains nickname fragments to check for to ignore when checking for user activity. In this case, if someone is nicknamed `user (exiled)`, they will not be eligible to receive tips.

**```"statusImages": [
    "https://i.imgur.com/6zJpNZx.png",
    "https://i.imgur.com/fM26s0m.png",
    "https://i.imgur.com/SdWh89i.png"
  ],```**

This is a list of images that will be embedded in status messages when a successful tip is made. The image chosen is randomized between these values.

**```"donationImages": [
    "https://i.imgur.com/SZgzfAC.png"
  ]```**

This is a list of images that will be embedded in status messages when the bot's balance is insufficient to tip. In this case, since `tipMin` is set to 10, if the balance was 5, one of these images would be posted to all status channels along with a message saying that the bot has insufficient funds.
