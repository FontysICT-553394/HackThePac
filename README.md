# PacSabotage (Hack The Pack)
Welcome to the project Hack The Pac (formely PacSabotage)! Hac The Pac is a Fontys University of Applied Science owned and developed project, made for public educational events, like Fontys' very own open days.

In Hack The Pac, visiters of these events can play a round of Pac-Man in a group of 4. Players can take control of either a Pac-Man or a Ghost in a team based battle against one another. The users get to change many game settings before the round starts.

The goal of this project is to entice the user to understand the many aspects that go into IT-development, both in tooling and in decision-making.

## Getting Started
In order to get a proper understanding of what this project entails, we have included 3 .docx files
we recommend you read, use and manage during production.

- [ ] [Debriefing:](https://git.fhict.nl/I511494/pacsabotage/-/raw/main/Documentation/Debriefingsdocument.docx?ref_type=heads) This document contains a quick overview of what this project is about, generally used for pitching the project to other parties outside of the development team. Keep in mind that this document is written in DUTCH!
- [ ] [Operation Manual](https://git.fhict.nl/I511494/pacsabotage/-/raw/main/Documentation/Hack_The_Pac_Operation_Manual.docx?ref_type=heads) This document explains in detail how to operate the hardware and software, along with a detailed explanation of each menu and control option. This document is generally used for event organizers with little to no context of the project, and for new members of the projectteam. Keep in mind that this document is written in ENGLISH!
- [ ] [Transfer Document](https://git.fhict.nl/I511494/pacsabotage/-/raw/main/Documentation/Hack_The_Pac_Overdrachtsdocument.docx?ref_type=heads) This document explains what the previous projectteam has been working on, with what tools and (most importantly) what needs to be iterated on by the next development team. This document is primarily for new member of the development team and old members who need a quick refresher. Keep in mind that this document is written in DUTCH!

## Set-Up Guide
If you want to work on this game, you can follow the following steps.

### Requirements:
In order to build, develop, compile or control the project, you must need the following:
- [ ] [The latest version of Unity Hub](https://unity.com/download)
- [ ] An IDE to program C# code in, like [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) [ (with the Unity SDK installed)](https://youtu.be/uGZfdxp72do?si=ck5OrTLyJR4haxPb) or [Visual Studio Code](https://visualstudio.microsoft.com/downloads/)
- [ ] A controller that can be recognized as a Joystick controller, like the [Atari CX40+ Wireless Joystick Controller](https://www.bol.com/nl/nl/p/pac-man-cx-40-draadloze-joystick-geel/9300000237524906/?Referrer=ADVNLGOO002012-S--9300000237524906-PMAX-C-22290422363&gad_source=1&gad_campaignid=22294177366&gbraid=0AAAAAD5OnmMd6wNinfcv3BwEMoSvRY7kJ&gclid=EAIaIQobChMI2brekYSPkgMVi6aDBx1NmxSPEAQYAiABEgIWD_D_BwE)

### Clone Repository

To clone the repository, be sure to use your GIT credentials first!

```
git config --global --local user.name YOUR_USERNAME 
git config --global --local user.password YOURTOKEN
git clone https://git.fhict.nl/I511494/pacsabotage.git
```

Of course, what tool you clone the repository with is up to you. You can use Git Bash, .cmd or even built in GIT GUI's like the one included in Visual Studio 2022 or Unity Hub

### Open in Unity
Once you have cloned the repository onto your device, you can open up the project in Unity. First, open up the Unity Hub. Once done, you'll find an option to add a project on the top right of the screen, at which point, you can do the following:

- [ ] Select add from disk. Locate your cloned repository folder and select it.
- [ ] Optional: You can also add from repository, in which case you'll have to set up your GIT account in Unity Hub. You'll have to figure this out on your own unfortunately.

If done correctly, your game will now open up in the editor! This may take a few minutes to properly load up.

If you want to program your scripts (.cs) files, you need to open them with your IDE. If Unity has properly installed, built-in support for your IDE (like with Visual Studio 2022), you can double-click the script file in the Unity editor to launch the application instead.

### Add your files
- [ ] [Add files using the command line](https://docs.gitlab.com/ee/gitlab-basics/add-file.html#add-a-file-using-the-command-line) or push an existing Git repository with the following command:

```
cd existing_repo
git remote add origin https://git.fhict.nl/I511494/pacsabotage.git
git branch -M BRANCH_OF_YOUR_CHOOSING(i.e. main)
git push -uf origin BRANCH_OF_YOUR_CHOOSING(i.e. main)
```

Again, what tool you push your code with is up to you. You can use Git Bash, .cmd or even built in GIT GUI's like the one included in Visual Studio 2022 or Unity Hub.

### End
You will now have properly set up your working environment! Have fun programming! 

## License
This project is made by students and organized by Fontys University Of Applied Science for educational purposes only.
Unauthorized redistribution or use is prohibited.

Pac-Man and all related names, character logos and trademarks are owned and copyrighted by Bandai Namco Entertainment Inc.
This project is not endorsed by -or affiliated with Bandai Namco Entertainment Inc.
