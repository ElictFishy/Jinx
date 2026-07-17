using JinxApp.DataService;
using JinxApp.Managers;
using JinxApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
namespace JinxApp
{
    public static class JinxData
    {
        public static Game? g {  get; set; }
        public static GameManager? gm{  get; set; }
        public static IDataService? dataService { get; set; }

        /// <summary>
        /// Vrai quand la partie en cours est une démonstration (exemple de partie
        /// jouée par deux IA, lancée depuis la page des règles). Dans ce mode, la
        /// partie n'est jamais sauvegardée : ni partie courante, ni historique,
        /// ni classement.
        /// </summary>
        public static bool IsDemo { get; set; }
    }
}
