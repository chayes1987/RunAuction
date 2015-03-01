# RunAuction
This is the run auction service for my FYP. It is written in VB.net. It uses a 0mq binding. It contains the main algorithm
for the project. It is responsible for running the auction. It is responsible for running the auction.

## Project Setup

Requires a CouchDB instance running on port 5984.

Database: auctions

Sample Items:
{
   "_id": "1",
   "_rev": "1-194f699921e40fc5fe4f35420fe22fd6",
   "estimated_value": 1500,
   "starting_bid": 2000
}

{
   "_id": "2",
   "_rev": "1-8f297ae9d7cfbc2096b638d51ef3d174",
   "estimated_value": 699,
   "starting_bid": 1000
}

## License

None

## Setting up StartAuction service on AWS

Created AWS EC2 Windows instance
Connected with RDP and .pem keyfile
Installed CouchDB an configured database
Copied Service build to instance
Configure Windows Firewall to allow port access for 0mq

Service runs and works as expected
