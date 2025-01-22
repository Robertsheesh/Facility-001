VAR npc_name = "Dr. Sofia Carter"

-> root

=== root ===
You're awake... How... how is that possible?

+ Who are you? 
	-> who_are_you

+ Where am I? 
	-> where_am_i

+ What the fuck is happening? 
	-> what_is_happening

=== who_are_you ===
You know me... or at least, you should. Listen, I don’t have much time. You need to get to the northern observatory.

+ Where am I? 
	-> where_am_i

+ What the fuck is happening? 
	-> what_is_happening 

+ Can I trust you? 
	-> can_i_trust_you

=== where_am_i ===
You're in what's left of the crew quarters. It used to be a safe zone, but things... changed. We had to abandon it.

+ Did anyone else survive? 
	-> did_survive

+ Can I trust you? 
	-> can_i_trust_you

+ What the fuck is happening? 
	-> what_is_happening 

=== what_is_happening ===
Look, I don't have all the answers for you. You need to find a keycard to get through the airlock. It might need to be rewritten, but once you're on the other side, find a computer and message me. I'll guide you from there.

+ Can I trust you? 
	-> can_i_trust_you

+ Where do I find the keycard? 
	-> keycard_location


=== did_survive ===
I... I don't know. We lost contact weeks ago. Some tried to escape. Most didn't make it.

+ Can I trust you? 
	-> can_i_trust_you

=== can_i_trust_you ===
Do you have a better option? You have to get out of there! It's not safe.

+ Why should I trust you? 
	-> why_trust

+ What the fuck is happening? 
	-> what_is_happening 

=== on_it ===
Good. Move quickly, and stay quiet. I’ll do my best to guide you, but you're on your own from here.

-> END

=== why_trust ===
I stayed behind to help. That's all I can say for now. Just... trust me. You have to hurry!

+ What the fuck is happening? 
	-> what_is_happening 

=== another_way ===
There might be. But time isn’t on your side. Stick to the plan, and don’t wander.

+ I'm on it. 
	-> on_it

+ What the fuck is happening? 
	-> what_is_happening 

+ Where do I find the keycard? 
	-> keycard_location

=== keycard_location ===
Ethan was the last one to have it. His room’s down the hall. He had a habit of hiding things, so you might have to dig around a bit. I’m counting on you to find it.

+ How do I rewrite it? 
	-> rewrite_keycard

=== rewrite_keycard ===
Just insert it into this computer. Do I really need to spell out the rest for you?

+ I'm on it. 
	-> on_it

