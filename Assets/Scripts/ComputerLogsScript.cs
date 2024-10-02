using System.Collections.Generic;
using UnityEngine;

public class ComputerLogs : MonoBehaviour
{
    private List<LogEntry> logs = new List<LogEntry>();
    private bool hasAccessToLogs = false;
    private string correctSecurityCode = "1234"; // Security code to access the logs

    void Start()
    {
        // Add logs with number, date, title, and content
        logs.Add(new LogEntry(1, "February 1, 2099", "Woke up here like the others", "Author: Dr. Ethan Kessler\n\n\"Woke up here like the others. No warning, no explanation. Just... here. The others say the same thing. None of us remember arriving. None of us remember anything before this place. It’s like... we were always here. But that can’t be right. We had lives before this, didn’t we? The problem is, I can’t remember mine. I try, but it slips away like sand through my fingers. The more I think about it, the more I feel like maybe I was always meant to be here, and that scares me.\""));

        logs.Add(new LogEntry(2, "May 5, 2099", "Johnson's starting to worry me", "Author: Dr. Ethan Kessler\n\n\"Johnson’s starting to worry me. He’s always been a bit paranoid, but now it’s something else. He keeps talking about how none of this is real, how we’re all part of some grand experiment, watched by something we can’t see. I thought it was just the isolation getting to him, but now... I’m not so sure. The things he says—sometimes they make sense, in a way I can’t explain.\n\nI’ve started having these headaches. They come in waves, like fever dreams. And sometimes, just for a second, I feel like I’m not in control of myself. Like I’m just watching everything happen. I don’t know if I should be worried, but something tells me I should be.\""));

        logs.Add(new LogEntry(3, "August 3, 2099", "Tried to talk to Johnson today", "Author: Dr. Ethan Kessler\n\n\"I tried to talk to Johnson today. He won’t listen anymore. Says he’s found the truth, that we’re all just lab rats in some kind of twisted experiment. He keeps saying that ‘they’ are watching us, studying us. But he won’t say who ‘they’ are. He’s getting worse, too. Stays up all night staring at the walls like he’s waiting for something.\n\nThe headaches are getting worse. I can’t concentrate for more than a few minutes without feeling like I’m going to pass out. Sometimes it feels like the walls are closing in on me. I can’t even look outside the windows anymore. I don’t know what’s out there, but I’m starting to think there’s nothing at all.\""));

        logs.Add(new LogEntry(4, "October 12, 2099", "Johnson's gone", "Author: Dr. Ethan Kessler\n\n\"Johnson’s gone. He stormed out after another one of his rants about the experiment and how we’re all trapped here. Said he was going to the storage wing to get more fuel, but that was days ago. No one’s seen him since. I went down there to look for him, but... I couldn’t. The lights are dim, and the air... it’s different down there. Thicker, like something’s hanging in it, waiting for you to breathe it in.\n\nThe others say I should stop worrying. That we’re all going to end up like Johnson if we keep thinking about this place too much. But I can’t stop. I can’t shake the feeling that there’s something wrong with the factory. That we’re not just working here. We’re... part of it.\""));

        logs.Add(new LogEntry(5, "February 21, 2100", "I've stopped sleeping", "Author: Dr. Ethan Kessler\n\n\"I’ve stopped sleeping. Every time I close my eyes, the dreams come. Or maybe they’re not dreams anymore. I can’t tell. Sometimes it feels like I’m still dreaming even when I’m awake. The factory... it’s all I see, whether my eyes are open or closed. I don’t know how much longer I can keep doing this.\n\nJohnson isn’t coming back. I know that now. But I keep hearing things. At night, when everything goes quiet, I hear footsteps in the halls. Soft, like someone’s pacing just outside my door. But when I check, there’s no one there. There never is.\""));

        logs.Add(new LogEntry(6, "March 7, 2100", "Realized something", "Author: Dr. Ethan Kessler\n\n\"I’ve realized something. There’s no escape from this place, because there was never anywhere to escape to. The factory is all that exists. And we… we are just parts of it. I’ve tried to hold onto myself, tried to keep from losing who I am, but it’s getting harder. Every day, a little more of me slips away, and the factory fills the gap.\n\nI’m not even sure if I’m writing this anymore. Maybe it’s the factory speaking through me. Maybe it’s always been the factory. I think it’s time to stop fighting it. There’s no point. There’s only one way out, and I think I’m ready to take it.\""));
    }


    // LogEntry class for storing individual logs
    private class LogEntry
    {
        public int logNumber;
        public string date;
        public string title;
        public string content;

        public LogEntry(int number, string logDate, string logTitle, string logContent)
        {
            logNumber = number;
            date = logDate;
            title = logTitle;
            content = logContent;
        }
    }

    // Check if the player has access to logs
    public bool HasAccessToLogs()
    {
        return hasAccessToLogs;
    }

    // Enter the security code to access logs
    public bool EnterSecurityCode(string inputCode)
    {
        if (inputCode == correctSecurityCode)
        {
            hasAccessToLogs = true;
            return true;
        }
        return false;
    }

    // Return the list of available logs with log number, date, and title
    public List<string> GetLogList()
    {
        List<string> logList = new List<string>();
        foreach (var log in logs)
        {
            logList.Add($"Log #{log.logNumber} - {log.date} - {log.title}");
        }
        return logList;
    }

    // Get log by index
    public string GetLog(int index)
    {
        if (index >= 0 && index < logs.Count)
        {
            LogEntry log = logs[index];
            return $"Log #{log.logNumber} - {log.date}\n\n{log.content}";
        }
        return null;
    }
}
