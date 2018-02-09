# Tyrell
.NET Core 2.0 bot for Discourse API using ElasticSearch and NEST. Full forum post and thread indexing capabilities.

## SETUP:

Pretty much a plug and play discourse bot. You need ElasticSearch (I'm using 6.1.2), Discourse forum site URL, username, and password. My instance of the bot is called @Tyrell, 
feel free to rename where you need to. 

Edit the /Business/Constants.cs file with your relevant details. Spin up the bot and press 9 for automatic mode.

**PLEASE be respectful when using the full index commands. Just because you could index a full site doesn't mean you should**. It could
get your account banned pretty easily.

## FUNCTIONS:

### remindme

>remindme

The main reason I made it, every forum/site needs this functionality IMO. The implementation is still somewhat limited, but it’s a start.

**usage**:

>remindme tonight

>remindme tomorrow

>remindme [one-ten] [hour(s)/day(s)/week(s)/month(s)]

>remindme [number] [hour(s)/day(s)/week(s)/month(s)]

>remindme [HH][am/pm]

>remindme [HH:MM][am/pm]

>remindme [one-ten] [hour(s)/day(s)/week(s)/month(s)] (at) [HH][am/pm]

>remindme [number] [hour(s)/day(s)/week(s)/month(s)] (at) [HH][am/pm]

>remindme [one-ten] [hour(s)/day(s)/week(s)/month(s)] (at) [HH:MM][am/pm]

>remindme [number] [hour(s)/day(s)/week(s)/month(s)] (at) [HH:MM][am/pm]

>remindme [dd/mm/yyyy]

>remindme [some-time-function] [to/that] (something you want to be reminded of)


Reminders for tonight are at 6pm ONLY, tomorrow are at 7:30am ONLY, use the more advanced options to define more exact times. The rest are based on when you post the request. The (s) are completely optional. Usage of (at) and (in) is also optional.

**examples**:

remindme two days -> 2 days from now.

remindme 5 week -> 5 weeks from now.

remindme 1:30pm -> reminder 1:30 pm today.

remindme 1 days 7pm -> reminder tomorrow at 7pm.

remindme in 1 week 1:10pm -> reminder one week from today at 1:10pm.

remindme in 2 days at 4am -> reminder 4am 2 days from today.

remindme 15/01/2019 -> reminder on that date at 7:30am. You can’t give it a time YET.

remindme in 2 days at 9:30am to buy milk -> buy milk in 2 days at 9:30am.

remindme in 4 hours that I like coffee -> you guessed it.

You don’t need to @Tyrell the bot for remindme to work, it will get it from anywhere in a post.

### tyrell slap

>tyrell slap

Slaps a user, IRC style. Guess my childhood.

**usage**:

>tyrell slap [username]

you can @ the username if you want, it will add it if you don’t. Your post must call the bot with either @Tyrell or “tyrell”.

### tyrellignore

>tyrellignore

Useful for when you are posting something and you don’t want the bot to action anything in the post.

If you post something with a function or reminder and you are quoting another post make sure it doesn’t contain this flag, 
as **the flag presence anywhere in your post disables Tyrell on it**.

Once the bot has detected/processed your remindme or function usage it will like your post.

It runs every minute.

## KNOWN BUGS: 
- there is an issue with multiple commands in the same post.
- need to smooth out the pause automatic mode using CAPS handling.

## WISHLIST:
- Better datetime parsing

## GET CREATIVE:
Kibana (and/or many other) data visulisation tools are great fun once you have a dataset indexed of forum posts and threads.
