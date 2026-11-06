# Budgeteer - Ha koll på dina utgifter!

Budgeteer är en konsolapplikation skapat i C# .NET och gör det möjligt för dig att skapa grupper och skapa utgifter för att hålla koll på utgifter under perioder.

<img width="331" height="215" alt="image" src="https://github.com/user-attachments/assets/3a7ab228-83fa-423a-ac2c-06720acbf7fc" />

På förstasidan finns en överblick över dem grupper som skapats och deras namn. Med nummertangenterna kan du välja en grupp att gå in på och granska utgifterna tillhörande gruppen.
Likt utgifter kan du skapa, redigera och radera grupper när du vill. Raderar du en grupp försvinner samtliga utgifter tillhörande den gruppen!

Budgeteer fungerar genom att den tar inmatning från användare och efter validering serialiserar innehållet till .txt filer, istället för att ha en databas i någon server. Detta innebär att din information är säker, men att den endast fungerar offline.
Grupper och utgifter indexeras enligt ett unikt grupp- och utgiftsID som skapas automatiskt. Utgifterna får gruppens ID och sitt eget ID för att vara unika i sin grupp. 
Raderas en grupp eller en utgift omindexeras innehållet så det stämmer överens igen.

<img width="263" height="279" alt="image" src="https://github.com/user-attachments/assets/805c65f8-1cb4-40cc-93a4-5097c973eb09" />
