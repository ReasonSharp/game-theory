using System;
using System.Collections.Generic;
using System.Linq;

public enum Move {
 Cooperate,
 Deflect
}

public class PlayerMove {
 public Player Player { get; set; }
 public Move Move { get; set; }
}

public class HistoricalRecord {
 public PlayerMove Player1Move { get; set; }
 public PlayerMove Player2Move { get; set; }
}

public class Player {
 public Guid ID { get; } = Guid.NewGuid();
 public string Name { get; }
 public Func<Guid, List<HistoricalRecord>, Move> Strategy { get; }
 public Player(string name, Func<Guid, List<HistoricalRecord>, Move> strategy) {
  Name = name;
  Strategy = strategy;
 }
}

public class PlayerScore {
 public Player Player { get; set; }
 public uint Points { get; set; }
}
public class PlayerMoveScore : PlayerScore {
 public Move Move { get; set; }
}

public class Game {
 public Player player1;
 public Player player2;
 public List<HistoricalRecord> History { get; set; } = [];
 public Game(Player p1, Player p2) {
  player1 = p1;
  player2 = p2;
 }
 public List<PlayerMoveScore> PlayTurn() {
  Move p1Move = player1.Strategy(player1.ID, History);
  Move p2Move = player2.Strategy(player2.ID, History);
  History.Add(new HistoricalRecord {
   Player1Move = new PlayerMove { Player = player1, Move = p1Move },
   Player2Move = new PlayerMove { Player = player2, Move = p2Move }
  });
  uint p1Points, p2Points;
  if (p1Move == Move.Cooperate && p2Move == Move.Cooperate) {
   p1Points = 3;
   p2Points = 3;
  } else if (p1Move == Move.Cooperate && p2Move == Move.Deflect) {
   p1Points = 0;
   p2Points = 5;
  } else if (p1Move == Move.Deflect && p2Move == Move.Cooperate) {
   p1Points = 5;
   p2Points = 0;
  } else {
   p1Points = 1;
   p2Points = 1;
  }
  return [ new PlayerMoveScore { Player = player1, Move = p1Move, Points = p1Points }, new PlayerMoveScore { Player = player2, Move = p2Move, Points = p2Points } ];
 }
}

public class Tournament {
 public string Name { get; set; } = "Tournament";
 public List<Game> Games { get; set; } = [];
 public Dictionary<Guid, PlayerScore> Scoreboard { get; set; } = [];
 private uint rounds;

 public Tournament(string name, List<Player> players, int minRounds = 150, int maxRounds = 250) {
  Name = name;
  initGames(players);
  initScoreboard(players);
  rounds = (uint)(new Random((int)DateTime.Now.Ticks).Next(minRounds, maxRounds));
 }

 void initGames(List<Player> players) {
  if (players.Count < 2) return;
  for (var x = 0; x < players.Count - 1; ++x)
   for (var y = x + 1; y < players.Count; ++y)
    Games.Add(new Game(players[x], players[y]));
 }
 void initScoreboard(List<Player> players) {
  foreach (var player in players)
   Scoreboard.Add(player.ID, new PlayerScore { Player = player, Points = 0 });
 }

 public void Play() {
  Console.WriteLine($"Playing {rounds} rounds tournament:");
  foreach (var game in Games) {
   var pl1pts = 0u;
   var pl2pts = 0u;
   for (var x = 0; x < rounds; ++x) {
    var result = game.PlayTurn();
    Scoreboard[result[0].Player.ID].Points += result[0].Points;
    Scoreboard[result[1].Player.ID].Points += result[1].Points;
    pl1pts += result[0].Points;
    pl2pts += result[1].Points;
    Console.WriteLine($"Game {game.player1.Name} vs. {game.player2.Name} round {x + 1}: {result[0].Move} - {result[1].Move} => {result[0].Points} - {result[1].Points}");
   }
   Console.WriteLine($"Game {game.player1.Name} vs. {game.player2.Name} finished: {pl1pts} - {pl2pts}");
  }
  Console.WriteLine("Tournament complete");
 }
}

class Program {
 static Func<Guid, List<HistoricalRecord>, Move> titForTat = (id, history) => {
  if (history.Count == 0) return Move.Cooperate;
  var amPlayer1 = history[history.Count - 1].Player1Move.Player.ID == id;
  return amPlayer1 ? history[history.Count - 1].Player2Move.Move : history[history.Count - 1].Player1Move.Move;
 };
 static Func<Guid, List<HistoricalRecord>, Move> fairNSquare = (id, history) => {
  if (history.Count == 0) return Move.Cooperate;
  var amPlayer1 = history[history.Count - 1].Player1Move.Player.ID == id;
  var theirresponse = amPlayer1 ? history[history.Count - 1].Player2Move.Move : history[history.Count - 1].Player1Move.Move;
  if (history.Count == 1) return theirresponse;
  var myprompt = amPlayer1 ? history[history.Count - 2].Player1Move.Move : history[history.Count - 2].Player2Move.Move;
  var theirprompt = amPlayer1 ? history[history.Count - 2].Player2Move.Move : history[history.Count - 2].Player1Move.Move;
  if (myprompt == Move.Deflect && theirprompt == Move.Deflect) return Move.Deflect;
  if (myprompt == Move.Deflect && theirprompt == Move.Cooperate) return Move.Cooperate;
  return theirresponse;
 };
 static Func<Guid, List<HistoricalRecord>, Move> random = (id, history) => {
  return new Random((int)DateTime.Now.Ticks).Next() % 2 == 0 ? Move.Cooperate : Move.Deflect;
 };
 static Func<Guid, List<HistoricalRecord>, Move> alwaysCooperate = (id, history) => { return Move.Cooperate; };
 static Func<Guid, List<HistoricalRecord>, Move> alwaysDeflect = (id, history) => { return Move.Deflect; };
 static Func<Guid, List<HistoricalRecord>, Move> deflectTitForTat = (id, history) => {
  if (history.Count == 0) return Move.Deflect;
  var amPlayer1 = history[history.Count - 1].Player1Move.Player.ID == id;
  return amPlayer1 ? history[history.Count - 1].Player2Move.Move : history[history.Count - 1].Player1Move.Move;
 };
 static Func<Guid, List<HistoricalRecord>, Move> twopercent = (id, history) => {
  if (history.Count == 0) return Move.Cooperate;
  var amPlayer1 = history[history.Count - 1].Player1Move.Player.ID == id;
  var rnd = new Random((int)DateTime.Now.Ticks).Next(0, 100);
  if (rnd == 25 || rnd == 75) return Move.Deflect;
  return amPlayer1 ? history[history.Count - 1].Player2Move.Move : history[history.Count - 1].Player1Move.Move;
 };
 static Func<Guid, List<HistoricalRecord>, Move> grudge = (id, history) => {
  if (history.Count == 0) return Move.Cooperate;
  var amPlayer1 = history[history.Count - 1].Player1Move.Player.ID == id;
  if (amPlayer1 ? history.Any(X => X.Player2Move.Move == Move.Deflect) : history.Any(X => X.Player1Move.Move == Move.Deflect)) return Move.Deflect;
  return Move.Cooperate;
 };
 static Func<Guid, List<HistoricalRecord>, Move> antiTitForTat = (id, history) => {
  if (history.Count == 0) return Move.Cooperate;
  var amPlayer1 = history[history.Count - 1].Player1Move.Player.ID == id;
  return (amPlayer1 ? history[history.Count - 1].Player2Move.Move : history[history.Count - 1].Player1Move.Move) == Move.Deflect ? Move.Cooperate : Move.Deflect;
 };
 static Func<Guid, List<HistoricalRecord>, Move> nastyAntiTitForTat = (id, history) => {
  if (history.Count == 0) return Move.Deflect;
  var amPlayer1 = history[history.Count - 1].Player1Move.Player.ID == id;
  return (amPlayer1 ? history[history.Count - 1].Player2Move.Move : history[history.Count - 1].Player1Move.Move) == Move.Deflect ? Move.Cooperate : Move.Deflect;
 };
 static void Main(string[] args) {
  List<Player> players = [
   new Player("Tit4Tat", titForTat),
   new Player("Fair'n'Square", fairNSquare),
   new Player("Randoom", random),
   new Player("Coop", alwaysCooperate),
   new Player("Deflector", alwaysDeflect),
   new Player("Nasty Tit4Tat", deflectTitForTat),
   new Player("2% deflect", twopercent),
   new Player("Grudge", grudge)
  ];

  var tourney1 = new Tournament("Tournament 1", players);
  tourney1.Play();
  printScoreboard(tourney1.Name, tourney1.Scoreboard);
 }

 static void printScoreboard(string tournament, Dictionary<Guid, PlayerScore> scoreboard) {
  Console.WriteLine("========================================");
  Console.WriteLine(tournament);
  Console.WriteLine("========================================");
  foreach (var score in scoreboard.Values)
   Console.WriteLine($"{score.Player.Name} - {score.Points} points");
  Console.WriteLine();
 }
}
