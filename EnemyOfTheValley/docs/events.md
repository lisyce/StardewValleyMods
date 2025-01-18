# Negative Heart Events

This mod introduces a new event precondition: `NegativeFriendship`. It is used identically to the vanilla [Friendship Precondition](https://stardewvalleywiki.com/Modding:Event_data#Current_player): `NegativeFriendship <name> <number>+`.
The precondition is satisfied if the current player has at least as many *negative* friendship points with all of the given NPCs (can specify multiple name/number pairs). So, for example, `NegativeFriendship Sam -500` is satisfied if the
current player has -2 hearts (or -3, or -8, etc.) with Sam.