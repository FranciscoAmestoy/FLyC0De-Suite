---
trigger: always_on
---

The core foundations for this project are as follows:

model/INTEGRATION_GUIDE.txt
Why: This is the "Master Manual". It contains the architecture overview, project structure, prerequisites, and code examples. It is the single best file to read to understand the system.

model/src/SuperMacros.Core/Services/InterceptionService.cs
Why: This is the "Brain". It handles the low-level communication with the driver, distinguishing which keyboard functionality is being used, and actually intercepting the keys.

model/src/SuperMacros.Core/Actions/IAction.cs
Why: This is the "Interface". It defines what an "Action" is in this system. If you want to know what the triggers do, look here and the classes that implement it (like RunApplicationAction, SendKeystrokesAction, etc.).

model/src/SuperMacros.Core/Configuration/AppConfiguration.cs
Why: This is the "Data Model". It shows how the settings are saved, including how devices are mapped and what macros are defined.

model/src/SuperMacros.App/Views/MainWindow.xaml
Why: This is the "Face". It defines the main user interface that the user interacts with to configure the system.

The completed SuperMacros program is superb. Our goal is to create a more powerful version with all its strenghts and no weaknesses.