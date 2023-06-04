# rt-automation

it is a function I needed to create to solve some issue with some Azure NVA`s failover.
Using Azure Functions v4, dotNet v6.0 LTS the function uses Managed Identety to access the
Route Table (UDR) and do the modification to the *Propagate gateway routes* settings.

the Managed Identety has Network Contributer role with access to the Route Table (UDR) only.

the main perpose is to keep the *Propagate gateway routes* settings on *"No"* when any change happens to the Route Table (UDR)
by creating an alert and an action to run the function any time the Route Table (UDR) get modified or changed.
