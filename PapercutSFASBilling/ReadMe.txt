Yale PaperCut SFAS Billing Service
==================================
These are instructions oriented toward the end user of the compiled PapercutSFASBilling Software.
For Developer oriented ReadMe, please the the ReadMe file in the root of the project space.

Set-Up
----------------------------------
Once the MSI has been run and the software installed, a folder must be designated that the service
can write to, enabling the service to generate Billing and Error files in that folder. The Config.txt
that was installed with this application should also be copied to that folder and filled out with all
relevant configuration information, as that is where the service will look for a configuration file.

When that has been completed, open the Service Manager and find the Papercut SFAS Billing Service. In
its properties dialog there should a "Start Parameters:" field, where you enter:
-p "E:\Papercut\Billing\Folder\Path\". This will give the service where the folder is as a runtime
parameter, failing to fill this out will just result in the service not attempting to do anything.