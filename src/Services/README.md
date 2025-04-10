#
The Services project is essentially a library skeleton. I recommend against using it as-is simply because more could be added to it. It lacks:
1) Database storage and retrieval of assistants and completions
2) Audit trail
3) A proper means of using generic <T>Assistants

There is no license on this code so feel free to copy-paste it at your own peril. I have included documentation where appropriate: more to follow.

The necessary components of responsibly using AI in production (in my opinion) is:
### Save details of created assistants
The possibility of changing things in production without a proper audit trail and notification system should terrify you, and a side-channel way of modifying things like https://platform.openai.com/assistants enables modifications to your customer-facing products. You MUST know at minimum when and by whom the assistant was created and modified. Ideally the system should have an email watchlist which should be notified when any assistant is modified. As of time of writing, platform.openai.com does NOT have either of those IMO mandatory features. 

This library is NOT necessary for creating assistants in the first place, as the UI is almost inarguably better. What this library does is enable in-channel auditing, logging, and persistance. **None of that is included in this codebase** but it is a skeleton to build on top of. 

### Save details of completions
Persistant output (output that would be saved in the DB and used multiple times) may become outdated due to changes in business requirements *even if the data used in the prompt did not change*. One example could be new legal regulations that require the output to conform to certain language. This means that your persistant storage of these completions should store metadata about its creation that can be used to filter the completions for later hiding or regeneration. Minimally this should include assistant name and assistant version (or last modified date) alongside whatever identifies the prompt used. 

### Save details of prompts
For reasons previously mentioned, prompts should also be versioned. The details of how to do this are left to your particular implementation, as prompts can be complicated. If you look at the code you may notice that it is possible to provide very complex data to the initialMessage, including multi-channel files, and that threads can be used multiple times to create a conversation with context about previous messages. A reader who has gotten this far in this crazy rant of mine should probably be able to know what and why is needed to create a proper audit trail for their implementation. 