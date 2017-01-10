# MysteryGiftTool
![License](https://img.shields.io/badge/License-GPLv3-blue.svg)

Automatic tracking/downloading/decryption/extraction of Gen VI/VII Mystery Gifts/PGL Regulations.

Note: Automatic decryption/extraction requires configuring a console on your local network to run [my crypto server](https://github.com/SciresM/3ds-crypto-server). It also requires you to edit the static "crypto_ip" and "crypto_port" variables in [NetworkUtils.cs](/MysteryGiftTool/NetworkUtils.cs) to point at that 3ds. I highly recommend selecting a static IP address for your console through System Settings should you choose to use my server often.

Note: MysteryGiftTool makes use of [PKHeX-Core](https://github.com/kwsch/PKHeX).

![Example of extracted mystery gifts](/img/example.PNG)
