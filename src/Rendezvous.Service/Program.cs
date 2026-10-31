using WhiteP.Rendezvous.Service;

var app = PeerAnnouncementServiceHost.Create(args);
await app.RunAsync();
