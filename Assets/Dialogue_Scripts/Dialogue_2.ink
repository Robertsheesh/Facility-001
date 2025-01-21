VAR npc_name = "Dr. Alex Mitchell"

-> root

=== root ===
You're finally online... I was starting to lose hope.

+ Who are you? 
    -> who_are_you

+ What is this place? 
    -> what_is_this_place

+ How did I get here? 
    -> how_did_i_get_here

=== who_are_you ===
I’m Dr. Alex Mitchell. I worked here... before things went south. I stayed to help, but time is running out.

+ What is this place? 
    -> what_is_this_place

+ How did I get here? 
    -> how_did_i_get_here

+ Can I trust you? 
    -> can_i_trust_you

=== what_is_this_place ===
This was once a research facility. Now? It's a tomb. We were working on something... dangerous.

+ What were you researching? 
    -> research_details

+ How did I get here? 
    -> how_did_i_get_here

+ Can I trust you? 
    -> can_i_trust_you

=== how_did_i_get_here ===
You were found outside, unconscious. We brought you in and patched you up. That's all I know.

+ What were you researching? 
    -> research_details

+ Can I trust you? 
    -> can_i_trust_you

=== research_details ===
We were studying an anomaly... something we didn't fully understand. It got out of hand.

+ Can I trust you? 
    -> can_i_trust_you

+ What should I do now? 
    -> next_steps

=== can_i_trust_you ===
I know how this sounds, but I'm your best shot at getting out of here alive. You don’t have much choice.

+ Why should I trust you? 
    -> why_trust

+ What should I do now? 
    -> next_steps

=== why_trust ===
I have nothing to gain by lying to you. Either we work together, or you stay here and wait for whatever comes next.

+ What should I do now? 
    -> next_steps

=== next_steps ===
There's an emergency exit in the lower levels. You'll need a security card to access it.

+ Where can I find a security card? 
    -> security_card_location

+ What happens if I stay? 
    -> stay_risks

=== security_card_location ===
Dr. Lewis had the last known keycard. His office is near the lab entrance. Check the drawers.

+ What happens if I stay? 
    -> stay_risks

+ I'm on it. 
    -> on_it

=== stay_risks ===
The anomaly is spreading. It's only a matter of time before this place becomes completely uninhabitable.

+ I'm on it. 
    -> on_it

=== on_it ===
Good. Move fast and stay alert. I'll monitor your progress from here.

-> END
