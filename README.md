# VeeamTestTask
To achieve result I imlemented in my solution follow patterns
-Producer Consumer pattern
-Thread Pool pattern
-Retry pattern

App safely handles next situations:
-FileAlreadyExists - gives user a choice, override or cancel app execution
-NotEnoughSpace - retry pattern - 30 attempts each 2 seconds, after that app fails
FileBusyByAnotherProcess - retry pattern -  if we unpack into executable file, sometimes antivirus can busy file until process will be finished, 30 attempts each 2 seconds

In project I use type matching for exception handling, because of it needs to be installed latest VS 2019(version 16.8) or .Net  5.0 SDK (5.0.100)
