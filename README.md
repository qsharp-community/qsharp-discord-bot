# qsharp-discord-bot
A discord bot that can evaluate Q# code and look up Q# docs for operations and functions in chat.

## Current status: MVP
- Goal is to get a minimal working example of the bot connected to discord and an azure webapp host with a Blazor based web portal for monitoring the system health. There are still a lot of good features/ideas/polish that is needed but we are getting there 🦾✨

### Wishlist:
- CI pipline to auto publish bot updates to azure
- Nicer formatting of latex in help command
- Possibly add a stateful execution mode indiviual to each user

## Notes:

```
az webapp up --name qsharp-discord-bot --sku free
```
