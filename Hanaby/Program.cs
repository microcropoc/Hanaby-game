namespace Hanaby
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    enum ColorCard { White, Red, Green, Blue, Yellow}

    internal class Card
    {
        internal ColorCard Color
        {
            get { return _color; }
        }

        ColorCard _color;

        internal int Rank
        {
            get { return _rank; }
        }

        int _rank;

        public bool VisibleColor
        {
            get { return _visibleColor; }
            set { _visibleColor = value; }
        }

        bool _visibleColor;

        public bool VisibleRank
        {
            get { return _visibleRank; }
            set { _visibleRank = value; }
        }

        bool _visibleRank;

        public Card(ColorCard color, int rank)
        {
            _color = color;
            _rank = rank;
        }

        public bool IsNextAfter(Card currentCard)
        {
            return this.Rank - currentCard.Rank == 1;
        }

        public string ToStringForPlayer()
        {
            return (_visibleColor ? Color.ToString()[0] : '#') + (_visibleRank ? Rank.ToString() : "#");
        }

        public override string ToString()
        {
            return Color.ToString()[0]+Rank.ToString();
        }

    }

    internal class Player
    {
        internal List<Card> CardsPlayer
        {
            get
            {
                if (_cardsPlayer == null)
                    throw new Exception("Колода игрока не существует");
                return _cardsPlayer;
            }
            set { _cardsPlayer = value; }
        }

        List<Card> _cardsPlayer;

        public int CountCardsPlayed
        {
            get { return _countCardsPlayed; }
            set { _countCardsPlayed = value; }
        }

        int _countCardsPlayed;

        public int CountRiskCardsPlayed
        {
            get { return _countRiskCardsPlayed; }
            set { _countRiskCardsPlayed = value; }
        }

        int _countRiskCardsPlayed;

        int _countCard;

        public Player(int countCard,List<Card> deck)
        {
            if (deck.Count < countCard + 1)
                throw new Exception("В колоде не хватает карт для раздачи");

            _cardsPlayer = new List<Card>();

            for (int i = 0; i < countCard; i++)
            {
                Card selectCard=deck[deck.Count-1];
                _cardsPlayer.Add(selectCard);
                deck.Remove(selectCard);
            }

            _countCard = countCard;
        }

        public bool DropCard(int numDropCart,List<Card> deck)
        {
            if (deck.Count < 2 || _cardsPlayer.Count<numDropCart || numDropCart<1)
                return false;

            _cardsPlayer.RemoveAt(numDropCart-1);
            Card newCard = deck[deck.Count - 1];
            _cardsPlayer.Add(newCard);
            deck.Remove(newCard);
            return true;
        }

        public bool AddCardFromDeck(List<Card> deck)
        {
            if (deck.Count < 2 || _cardsPlayer.Count==_countCard)
                return false;

            Card newCard = deck[deck.Count - 1];
            _cardsPlayer.Add(newCard);
            deck.Remove(newCard);
            return true;
        }

        //Проверям, есть ли карта по такому номеру
        public bool IsCardExist(int numCard)
        {
            return _cardsPlayer.Count >= numCard && numCard>0;
        }

        public override string ToString()
        {
            return "Кол-во успешно разыгранных карт: "+CountCardsPlayed.ToString()+'\n'+"Кол-во рискованных ходов: "+CountRiskCardsPlayed.ToString();
        }
    }


    class Program
    {
        const int constCardInHand = 5;
        const int constCardInDeck = 56;

        static void NewGame()
        {
            //initialization
            List<Card> deck = CreateCardDeck(constCardInDeck);

            Player playerOne = new Player(constCardInHand, deck);

            Player playerTwo = new Player(constCardInHand, deck);

            Dictionary<char, List<Card>> table = new Dictionary<char, List<Card>>();

            #region initializationDictionary

            for (int indexColor = 0; indexColor < 5; indexColor++)
                table[((ColorCard)indexColor).ToString()[0]] = new List<Card>();

            #endregion

            //очерёдность хода, если false, то ходит первый игрок
            bool turnPlayer=false;
            //если false, то gameover
            bool statGame=false;
            //если отличен от truePlayer, то ход прошёл успешно и следует сменить игрока
            bool turnPlayerAfterCourse=false;
            
            do
            {
                //вывод игрового поля на экран
                ViewGameField(table, playerOne, playerTwo, deck.Count, turnPlayer);

                if(table.All(p=>p.Value.Count==5))
                {
                    Console.WriteLine("Больше нельзя сделать ход");
                    Console.ReadKey(true);
                    break;
                }

                //выбор действия игрока
                Console.WriteLine("Выберете действие");
                Console.WriteLine("1. Разыграть карту");
                Console.WriteLine("2. Сбросить карту");
                Console.WriteLine("3. Подсказать цвет");
                Console.WriteLine("4. Подсказать наименование");

                //Сохранение очерёдности хода до самого хода
                turnPlayerAfterCourse = turnPlayer;

                #region SwitchForSelectGameVariantCorse
                
                switch (Console.ReadKey(true).KeyChar)
                {
                    case '1':

                        ViewGameField(table, playerOne, playerTwo, deck.Count, turnPlayer);
                        ConsoleRunPlayCard(ref statGame, ref turnPlayer, turnPlayer ? playerTwo : playerOne, deck, table);

                        break;
                    case '2':

                        ViewGameField(table, playerOne, playerTwo, deck.Count, turnPlayer);
                        ConsoleRunDropCard(ref statGame, ref turnPlayer,turnPlayer ? playerTwo: playerOne, deck);

                        break;
                    case '3':

                        ViewGameField(table, playerOne, playerTwo, deck.Count, turnPlayer);
                        ConsoleRunPromtColor(ref statGame, ref turnPlayer, playerOne, playerTwo);

                        break;
                    case '4':

                        ViewGameField(table, playerOne, playerTwo, deck.Count, turnPlayer);
                        ConsoleRunPromtRank(ref statGame, ref turnPlayer, playerOne, playerTwo);

                        break;
                    default:
                        statGame = true;
                        break;
                }

                #endregion

                if (deck.Count <= 1)
                    break;
                
                //Запрос клавиши при смене игрока
                if(statGame&&(turnPlayer!=turnPlayerAfterCourse))
                {
                    Console.WriteLine("Для смены игрока нажмите любую клавишу");
                    Console.ReadKey(true);
                }

            } while (statGame);


            ViewGameOver(playerOne,playerTwo);

        }

        static void About()
        {
            Console.Clear();
            Console.WriteLine("Нумерация карт начинается с единицы");
            Console.ReadKey(true);
            Console.Clear();
        }

        #region NewGameSubMethods

        #region IOConsoleGameVariantCourse

        static void ConsoleRunPlayCard(ref bool statGame, ref bool turnPlayer, Player player, List<Card> deck, Dictionary<char, List<Card>> table)
        {
            int selectedNumCard;
            Console.WriteLine("Введите номер карты: ");

            try
            {
                selectedNumCard = int.Parse(Console.ReadKey(true).KeyChar.ToString());
            }
            catch
            {
                statGame = true;
                return;
            }

            if (player.IsCardExist(selectedNumCard))
            {
                statGame = PlayCard(player.CardsPlayer[selectedNumCard - 1], player, deck, table);
                turnPlayer = !turnPlayer;
            }
            else
            {
                Console.WriteLine("Карты с таким номером не существует у вас в руке");
                statGame = true;
            }

        }

        static void ConsoleRunDropCard(ref bool statGame, ref bool turnPlayer, Player player, List<Card> deck)
        {

            int selectedNumCard;

            Console.WriteLine("Введите номер карты: ");

            try
            {
                selectedNumCard = int.Parse(Console.ReadKey(true).KeyChar.ToString());
            }
            catch
            {
               // selectedNumCard = -1;
                statGame = true;
                return;
            }

            if (player.IsCardExist(selectedNumCard))
            {
                statGame = player.DropCard(selectedNumCard, deck);
                turnPlayer = !turnPlayer;
            }
            else
            {
                Console.WriteLine("Карты с таким номером не существует у вас в руке");
                statGame = true;
            }

        }

        static void ConsoleRunPromtColor(ref bool statGame, ref bool turnPlayer, Player playerOne, Player playerTwo)
        {
            
            int numColorOrRank;
            int[] selectedNumsCard;

            Console.WriteLine("Введите номер цвета карты : ");
            Console.WriteLine("1. White");
            Console.WriteLine("2. Red");
            Console.WriteLine("3. Green");
            Console.WriteLine("4. Blue");
            Console.WriteLine("5. Yellow");

            try
            {
                numColorOrRank = int.Parse(Console.ReadKey(true).KeyChar.ToString());
                if (numColorOrRank > 5)
                {
                    statGame = true;
                    return;
                }
            }
            catch
            {
                statGame = true;
                return;
            }

            Console.WriteLine("Введите номера карт с {0} цветом (можно использовать пробел как разделитель)", ((ColorCard)numColorOrRank - 1).ToString());

            string inputString = Console.ReadLine();

            try
            {
                inputString = inputString.Replace(" ", "");
                selectedNumsCard = new int[inputString.Length];
                for (int i = 0; i < inputString.Length; i++)
                    selectedNumsCard[i] = int.Parse(inputString[i].ToString());
            }
            catch
            {
                statGame = true;
                return;
            }

            if (statGame = PromtColorPlayer(((ColorCard)numColorOrRank - 1),turnPlayer? playerOne:playerTwo, selectedNumsCard))
            {
                turnPlayer = !turnPlayer;
            }
            else
            {
                Console.WriteLine("Вы назвали не верные карты");
                Console.ReadKey(true);
                statGame = false;
                return;
            }
          
        }

        static void ConsoleRunPromtRank(ref bool statGame, ref bool turnPlayer, Player playerOne, Player playerTwo)
        {

            //номер цвета или достоинсва карты
            int numColorOrRank;
            //номера карт подсказки цвета или достоинсва карты
            int[] selectedNumsCard;


            Console.WriteLine("Введите номер достоинсва карты карты : ");
            Console.WriteLine("1. 1");
            Console.WriteLine("2. 2");
            Console.WriteLine("3. 3");
            Console.WriteLine("4. 4");
            Console.WriteLine("5. 5");

            try
            {
                numColorOrRank = int.Parse(Console.ReadKey(true).KeyChar.ToString());
                if (numColorOrRank > 5)
                {
                    statGame = true;
                    return;
                }
            }
            catch
            {
                // numColorOrRank = -1;
                statGame = true;
                return;
            }

            Console.WriteLine("Введите номера карт с достоиством в {0} (можно использовать пробел как разделитель)", numColorOrRank.ToString());

            string inputString = Console.ReadLine();



            try
            {
                inputString = inputString.Replace(" ", "");
                selectedNumsCard = new int[inputString.Length];
                for (int i = 0; i < inputString.Length; i++)
                    selectedNumsCard[i] = int.Parse(inputString[i].ToString());


            }
            catch
            {
                statGame = true;
                return;
            }

            if (statGame = PromtRankPlayer(numColorOrRank,turnPlayer ? playerOne:playerTwo, selectedNumsCard))
            {
                turnPlayer = !turnPlayer;
            }
            else
            {
                Console.WriteLine("Вы назвали не верные карты");
                Console.ReadKey(true);
                statGame = false;
                return;
            }
        }

        #endregion

        #region ViewConsoleGameFieldAndGameOver
        static void ViewGameField(Dictionary<char, List<Card>> table, Player playerOne, Player playerTwo, int countCardInDeck, bool turnPlayer)
        {
            Console.Clear();

            if (turnPlayer)
            {
                Console.Write("PlayerOne: ");
                foreach (Card selectCard in playerOne.CardsPlayer)
                    Console.Write(selectCard + " ");
                
                ViewTable(table);

                Console.Write("PlayerTwo: ");
                foreach (Card selectCard in playerTwo.CardsPlayer)
                    Console.Write(selectCard.ToStringForPlayer() + " ");
            }
            else
            {
                Console.Write("PlayerTwo: ");
                foreach (Card selectCard in playerTwo.CardsPlayer)
                    Console.Write(selectCard + " ");

                ViewTable(table);

                Console.Write("PlayerOne: ");
                foreach (Card selectCard in playerOne.CardsPlayer)
                    Console.Write(selectCard.ToStringForPlayer() + " ");
            }
            Console.WriteLine('\n');
        }

        static void ViewTable(Dictionary<char, List<Card>> table)
        {
            Console.WriteLine();
            Console.WriteLine("---------------------------------------------------------------");
            Console.WriteLine("W\tR\tG\tB\tY");
            Console.WriteLine();
            for (int numRank = 1; numRank < 6; numRank++)
            {
                for(int indexColor = 0; indexColor < 5; indexColor++)
                    Console.Write("{0}\t", table[((ColorCard)indexColor).ToString()[0]].Any(p=>p.Rank==numRank)?((ColorCard)indexColor).ToString()[0]+numRank.ToString():" ");
                Console.WriteLine();
            }
            Console.WriteLine("---------------------------------------------------------------");
            Console.WriteLine();
        }

        static void ViewGameOver(Player playerOne, Player playerTwo)
        {
            Console.Clear();
            Console.WriteLine("               GameOver");
            Console.WriteLine("========================================");
            Console.WriteLine("PlayerOne: ");
            Console.WriteLine(playerOne.ToString());
            Console.WriteLine('\n');
            Console.WriteLine("PlayerTwo: ");
            Console.WriteLine(playerTwo.ToString());
            Console.ReadKey(true);
            Console.Clear();
        }

        #endregion

        #region ModelGameVarintСourse
        static bool PlayCard(Card playCard,Player player,List<Card> deck, Dictionary<char,List<Card>> table)
        {
            //Если на столе не сущесвует карты такого цвета, то проверяем достоинство карты
            if (table[playCard.Color.ToString()[0]].Count==0)
            {
                if (playCard.Rank == 1)
                {
                    player.CountRiskCardsPlayed += playCard.VisibleColor && playCard.VisibleRank ? 0 : 1;
                    player.CountCardsPlayed++;

                  //  table[playCard.Color.ToString()[0]] = new List<Card>();

                    table[playCard.Color.ToString()[0]].Add(playCard);
                    player.CardsPlayer.Remove(playCard);
                    //добор из колоды в руку
                    player.AddCardFromDeck(deck);
                    return true;
                }

                return false;
            }
            else
            {
                if (table[playCard.Color.ToString()[0]].Any(p => p.ToString() == playCard.ToString()))
                    return false;
                foreach (Card selectCard in table[playCard.Color.ToString()[0]])
                {
                    if (playCard.IsNextAfter(selectCard))
                    {
                        player.CountRiskCardsPlayed += playCard.VisibleColor && playCard.VisibleRank ? 0 : 1;
                        player.CountCardsPlayed++;

                        table[playCard.Color.ToString()[0]].Add(playCard);
                        player.CardsPlayer.Remove(playCard);
                        //добор из колоды в руку
                        player.AddCardFromDeck(deck);
                        return true;
                    }
                }
                return false;
            }
        }

        static bool DropCardFromCardsSelectPlayer(int numDropCart, List<Card> deck, Player selectPlayer)
        {
            return selectPlayer.DropCard(numDropCart, deck);
        }

        static bool PromtColorPlayer(ColorCard color,Player player, params int[] numbersCards)
        {
            if (numbersCards.Length == 0)
                if (player.CardsPlayer.Any(p => p.Color == color))
                    return false;

            for(int i=1;i<=player.CardsPlayer.Count;i++)
            {
                if (player.CardsPlayer[i - 1].Color == color)
                    //проверяет, правильно ли подсказаны карты
                    if (numbersCards.Any(p => p == i))
                    {
                        player.CardsPlayer[i - 1].VisibleColor = true;
                    }
                      else
                        return false;
            }

            return true;
        }

        static bool PromtRankPlayer(int rank, Player player, params int[] numbersCards)
        {
            if (numbersCards.Length == 0)
                if (player.CardsPlayer.Any(p => p.Rank == rank))
                    return false;

            for (int i = 1; i <= player.CardsPlayer.Count; i++)
            {
                if (player.CardsPlayer[i - 1].Rank == rank)
                    //проверяет, правильно ли подсказаны карты
                    if (numbersCards.Any(p => p == i))
                    {
                        player.CardsPlayer[i - 1].VisibleRank = true;
                    }
                    else
                        return false;
            }

            return true;
        }

        #endregion

        static List<Card> CreateCardDeck(int countCard = 11)
        {
            Random rand = new Random();
            List<Card> cardDeck = new List<Card>();
            for (int i = 0; i < countCard; i++)
                cardDeck.Add(new Card((ColorCard)rand.Next(0, 5), rand.Next(1, 6)));

            return cardDeck;
        }

        #endregion

        static void Main(string[] args)
        {
            char selectKey;

            do
            {
                #region MajorMenu
                Console.Clear();

                Console.WriteLine("1. Новая игра");
                Console.WriteLine("2. Справка");
                Console.WriteLine("3. Выход");

                selectKey = Console.ReadKey(true).KeyChar;
                Console.Clear();

                if (selectKey == '3')
                    break;

                switch (selectKey)
                {
                    case '1':

                        NewGame();

                        break;
                    case '2':

                        About();

                        break;
                }

                #endregion
                
            } while (true);

        }

    }
}
