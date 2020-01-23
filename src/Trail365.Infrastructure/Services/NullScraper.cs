using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;

namespace Trail365.Services
{
    public class NullScraper : MapScraper
    {
        public static string GetLogoPngAsBase64()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("iVBORw0KGgoAAAANSUhEUgAAAyAAAAMgCAYAAADbcAZoAAAAAXNSR0IArs4c6QAAAARnQU1BAACx");
            sb.Append("jwv8YQUAAAAJcEhZcwAAFiUAABYlAUlSJPAAAEL9SURBVHhe7d0/dttG2zfgm99ayPgkJ16A6BVQ");
            sb.Append("dqFKZdKRpaTCnVMlnQtRJdk9b6lKRUyuwNQC7BMfm9gLP4CCE8fxHwkkBwB5XefBE4iyZWlEkPPD");
            sb.Append("zD3TWeUCAAAggf9X/hcAAGDnBBAAACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAA");
            sb.Append("khFAAACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkBBAAACAZAQQAAEhGAEkhG0e/04lOrUc/xln5");
            sb.Append("/QAAQE0EEAAAIBkBBAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkBBAAACAZ");
            sb.Append("AQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACS");
            sb.Append("EUAAAIBkBBAAACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAAkhFAAACAZAQQAAAg");
            sb.Append("GQEEAABIRgABAACS6axy5flemI86cTwtP4AvGc5iNRmUHwAAkJIREAAAIBkBBAAASEYAAQAAkhFA");
            sb.Append("AACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkBBAAACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkB");
            sb.Append("BAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkOqtcec6uZOPo9y7itvywHkdx");
            sb.Append("uVzEebf8EAAAamAEBAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkBBAAACAZ");
            sb.Append("AQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAAkhFAAACAZAQQABppPupEp7PhMZqXX60d");
            sb.Append("tvIzpzpa1rZAcwggAABAMgIIAACQjAACAAAkI4AAAADJCCAAAEAyAggAAJCMAAIAACQjgAAAAMkI");
            sb.Append("IAAAQDICCAA0xGCyitUq8TEblv86QBoCCAAAkIwAAgAAJCOAAAAAyQggAABAMgIIAACQjAACAAAk");
            sb.Append("I4AA0EhbWZJ2Mii/GgBNIYAAAADJCCAAAEAyAggAAJCMAAIAACQjgAAAAMkIIAAAQDICCAAAkIwA");
            sb.Append("AgAAJCOAAAAAyQggAABAMgIIAACQjAACAAAkI4AAAADJCCAAAEAyAggAAJBMZ5Urz9mVbBz93kXc");
            sb.Append("lh/W4ygul4s475YfQhVZFvPln3Fz8y7evHmzfuj29ivP7KOj/FlXeByPH0f8eHISj3q96EU3um18");
            sb.Append("HuY/exbLWC4j3t/cxLv8oY9tkLdC3g7l6Zd8rS3yhnBJUrv5KDrH0/KDBxjOYjUZlB8A3J8AkoIA");
            sb.Append("Qmvlne75n/Hy9+uYfrOHXUHeKR8+Ps0744/i2WDQqI54loeN5TpoXech4zvhYmN5OBmexunJszgf");
            sb.Append("uECpgQACJGYKFjRdEWA7neg89BjNyy9QQTaPUb+ff51e9I4vth8+CvnXnE4v4uL49/gzKx+rSZb/");
            sb.Append("vOPxKPr9u7br9XpxXPzc012Hj0L+b6zbobf+t/ujccxrbo/vmY8+e6418hjFBlcAADskgAB/Kzri");
            sb.Append("o6IT3jveTehojCzm4/Hdz5p3Vnv5z3txMU0QNr6vCCPHvXYEEQCoQgAB1iMe41F/3RGf7mvu+DjK");
            sb.Append("sb473ovji2Jkp/xcA90FkX6MxlIIAPtFAIEDl83zTnkxArC3ySNXzHH/OMpRPtQOtzG96EV/NA8x");
            sb.Append("BIB9IYDAwcruRj2O29YpPzy30+PobVLTAwANIoDAIVrXevT2e9Rj3+QhpG86FgB7QACBQ7NeFnqP");
            sb.Append("az322O3FLyGDANB2AggckkbsSUN1t3Hxy1g9CACtJoDAoSimXQkflR0VGycOL+NyNovZstgRPT9W");
            sb.Append("qyj2cv3vUX4+/7OXw2Gx5+L23F7ES+UgALSYAAIHoQgfx1Fhr+ODdbQOG0WQuAsVi8UiJpPzOB8M");
            sb.Append("YtDtRrc4yj/7X+Xn8z97Ppnkf/culMwu8zBS/olNTH83CgJAewkgsPeyGPc3Dx9HR8MYXs5itu6U");
            sb.Append("f+Xu/3pkoPgzlzHc6m3/FI7yn+8yZh8DxzpsFEGi/PTGujE4z8NIEUSGG7bN7XWtu8cPJp/93nd6");
            sb.Append("zGJY/rsA7AcBBPZcNv4lLirNuzpajwL83SFfTGJyPojBulP+lbv/65GB4s+cx+S3x+WDzfbPz7jI");
            sb.Append("f77zyH+8HcuDyGQRy8tNQshtXNeZQABgAwII7LP5KHoPTh9HMSxGOfIOeTEKsPsOeR0+/ox3Ix11");
            sb.Append("/Izd881CyO31n6ZhAdBKAgjsrZsYHT9k4lUxBWm2Dh6TYpSjfHSvHA3XdR3r0Y4G/Izd8/+Lyhnk");
            sb.Append("9l0sy1MAaBMBBPbVdHr/uo+8Yz5bFlOQBvsZPOLHu+CxmKzrOpqjG+e/Va1weBPvDYEA0EICCBy4");
            sb.Append("o+EslnnHfD+nWpUGdwXljTQ4qVhkfRvvDIEA0EICCBywog5iMdnXUY+2GMRJxUGQN4ZAAGghAQQO");
            sb.Append("UlGEvVrXQVC/QcUEcmsIBIAWEkDg4BThoyjCLj+kfr0f899KBW/eWwkLgNYRQOCgCB+N1H0U7dg1");
            sb.Append("BQA2J4DAARE+9oyleAFoIQEEDsTR5TJt+BhM1juof/9YxPnBl6L04sdNNkYHgBYRQOAQDGex0MsH");
            sb.Append("ABpAAIF9d3QZS/OuAICGEEBgrw1jtji3zwcA0BgCCOyx4WwSxj4AgCYRQGBfDWdWvAIAGkcAScEa");
            sb.Append("/wAAsCaAAAAAyQggAABAMgLIwbiNd7ZMhu3Jsvx/85jPxzEejWI06ke/Xx6dTnQedPTi4rb8ugCw");
            sb.Append("5wSQJOxyDG1XhI3xeLQOGOvQ0OtFr3ccx8cXcTGdxnR6G7e35VH+HQDgvwQQgC/KYr4OHHejFEXY");
            sb.Append("uLiYrgMGAFCdAJJENx5ZBgtaIZuPY7QOHb04XgeO8hMAwFYIIAfkzfusPAP+rRztKEY6ji9iKnQA");
            sb.Append("wM4IIIn0FIFAA30MHuVoR/koALA7AghwmLJiqpXgAQCpCSCJdBtQBHJrHV7IZTEf9aPTM9UKAOog");
            sb.Append("gKTS+zFMwoKaZfO7UQ/JAwBqI4AckjfvQxk6BysbR793vLtRj6OjOBoOYzi8jNlslh/LWC7LY7WK");
            sb.Append("1TePZVy6QwHAgRBAUuk+itonYd1ex58SCIdoHT4utlrrcXQ0jMs8aCyXZYhYLGIxmcRkch6DwSA/");
            sb.Append("utHtlkf5dwAAASQhu6FDLbYYPo6K0Y3lch04FotJnOdBI88XAMADCCDJNGEzwttQh85hmcdo4/Bx");
            sb.Append("FMPL2Xoa1aIY3ZA4AGAjAkhCTdgLZHozL89g32Ux7h/HtPyoirsRj0VMzgemUQHAlgggCTVhKd6Y");
            sb.Append("/h5jdSAcgGz8S1xUHvo4iuFsWY54lA8BAFshgKTUiKV4TcPiAGTj+KVy+jiKy2LUQ/IAgJ0QQFLq");
            sb.Append("PovTBhSiT4/7RkHYa/OX1es+hrNFnMseALAzAkhSTShEL9zG9cu5PUHYT9k4fq9Y+HF0uYzJoPwA");
            sb.Append("ANgJASSxJhSiF26nx/FyLoKwf7I/ryuOfgzjN0MfALBzAkhi3WenDagDuTM97kV/bFUs9kkWf15X");
            sb.Append("ix9Hl8/D4AcA7J4AkloTdkT/xO3FcfRH48gMhrAPsj+jWv44itNnRj8AIAUBJLlBnAzL04a4nV5E");
            sb.Append("75dRGAyh9Zbvqk2/OjoN+QMA0hBAajBoWgIp3E7j4rgT/dHcaAitNb+pWH3++JGNBgEgEQGkDoOT");
            sb.Append("aGAEWSuK03u9viBCC2Xx/k15+kBHP/bKMwBg1wSQWjRvGta/3f4TRPqjmLc2iWSRzecxHo3yn6P4");
            sb.Append("Wb5w5EELAIB0BJCaNHIa1n/kQeR2Gse9XnQ6RWd9nIeR8lNNk4ekT8NGp9PJj170jo/jYjrNf47i");
            sb.Append("Z/nCUf519sEy3vmFAkDjCSB1GTyPy6asx3sveWd9epGHkaJj38k7+aMYjefr0ZFkmaT4t7L83yyD");
            sb.Append("xqgYwVgHjfzIQ9KnYQMAgGYSQGrTjWenrUog/1KMjEwvjtejI70yBBQjD6MiGBTBJA8J63Dy8Sj/");
            sb.Append("3j8++dz6KP/Ox3DxMWD8PZpRhoxe/m+WQWNqBIMtuX23LM8AgF0TQGrUPf+tscXoVRQjD9MiGBTB");
            sb.Append("JA8J63Dy8fgYIv4+Pvnc+ij/zsdw8TFgGM3g3nrxY9VM/+Z9upG8rXoT79taogXAwRJAajWI5+2a");
            sb.Append("hwX76fZd1DoGUnkDRQBoHwGkZvs2CgL16cajx+Xpg03jpqYF0bJxPzq9C9MJATgYAkjtBjGZiSCw");
            sb.Append("Db3Kc7DyCPL7OPE0rCzmo370LjaJHrehfAWAthFAmqB1K2JBM3WfnUblS+n2In4ZJ4og2ThG/V4c");
            sb.Append("Tzcf93ijCASAlhFAGqEb5/93Wb3jBNzpPotNFpe7vfgldptB7kY9iilXW8gea1bwAqBtBJCm6J7H");
            sb.Append("wlQs2NCmy1vfxkWvv5MQks2LTTK3M+rxL9ObsJ8/AG0igDTJYBJLc7FgI5sv7HAXQkZb2vY/mxfT");
            sb.Append("rTrROy42ySwf3Kpp/J5q6hgAbIEA0jDd80XMhkIIVLeN5a1vY3rcW+/4Xy2HZDEfj9Y79feOtzfd");
            sb.Append("6mtuL3pbC0wAsGsCSAMNJkIIbGJby1sXO/4f9zrRKXb5H4/L3f3LT35ivZt/uYt/v3+30ebxxbTC");
            sb.Append("0rpHcVTx0v8nMOXfS/nYneJ7nq8D0VhIAaABBJCGKkKI6VhQ1ZaXty52+b+4KHf3/3RH/7tjvZt/");
            sb.Append("uYt/5WlWR8OYLRex+K36930XmPLv5V/fX/E9H68D0cWNgnUA6ieANFgxHWs5szoWVNKimqqj4WUs");
            sb.Append("F5MYdPMPdrks95v3n42OAEB6AkjDdQfnsVgt49KULHiwu5qq8oNGOorhbBmLyXkU2eNON843GAX5");
            sb.Append("ptt3YQwEgLoJIK2Qd0iKKVmzy5BD4GEGk1Uza6rKKVeT9bDHZ3Y2CvIm7FsIQN0EkBYpRkMmi1Ue");
            sb.Append("RIamZcEDNKum6m7UY/VxytUX7Wpz0tuwbyEAdRNAWqg7mMRiVQQRIyJwX8V0rNWyzpqqozgazmK5");
            sb.Append("+sqox+dsTgrAnhJAWuzjiMhquVxPMZFF4DuKTv3q7npJaR08ihWuJoNPaj3uoSik33IIeWMOFgA1");
            sb.Append("E0D2Qbe7nmJSjIqslrO4vBxW3kvgYOQNdHQ0jOGPvfIBDsfd9bK+VnYZRPLn2PCyGPFY3QWPByWP");
            sb.Append("fxQjntscubk1BwuAmnVWufKcvVNsjraMP29u4vrNBvsTtNbdpm6PHz+OH388iUePiv0QupU7guyr");
            sb.Append("Ytfyl/H79TaukSJ0nMbJs/Nv1HdUlX+fo1/i9+ntgzc4LML26W/P41n+TXn6A1A3AeTQFLskL/NQ");
            sb.Append("8v4m3l2/iTd5V6a1waQYxcj/UwSMPGHEyaNHkSeM6OVdLCGDqopdw//8M78+3uXXx5v8gfwC+dIl");
            sb.Append("crQeZnwcj0+L596z6CXs3Gfzcfx58y6u82/w9rMLeP195dfEaR66nz2rPvICALsigFDKg0kxNTwP");
            sb.Append("J8UEjfdFQHm3/kTeCSt6Yf/4vMOzibtO3KfyDl2eJ+7kHbuTPFQU1sGiIFwAALSZAAIAACSjCB0A");
            sb.Append("AEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACSEUAA");
            sb.Append("AIBkBBAAACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAAkhFAAACAZAQQAAAgGQEE");
            sb.Append("AABIRgABAACSEUAAAIBkBBAAACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAAkhFA");
            sb.Append("AACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkBBAAACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkB");
            sb.Append("BAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkBBAAACAZAQQAAEhGAAEAAJIR");
            sb.Append("QAAAgGQEEAAAIBkBBAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkBBAAACAZ");
            sb.Append("AQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACS");
            sb.Append("EUAAAIBkBBAAACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAAkhFAAACAZAQQAAAg");
            sb.Append("GQEEAABIRgABAACSEUAAAIBkBBAAACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAA");
            sb.Append("khFAAACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkBBAAACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAA");
            sb.Append("IBkBBAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkBBAAACAZAQQAAEhGAAEA");
            sb.Append("AJIRQAAAgGQEEAAAIBkBBAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkBBAA");
            sb.Append("ACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgAB");
            sb.Append("AACSEUAAAIBkBBAAACAZAQQAAEims8qV57AVWZbFcrmM+PAhbv76K+Lt23hbfm6xWJRnD9WPfr88");
            sb.Append("jZ/j55+L//4UP538ED9EL3q9/MNuN7rFwwAANJYAwkaybB7LV3nQuL6Ot3m4qBovtipPKv11SPkp");
            sb.Append("TvKA0svTSTcPJwAA1E8A4YGymF+9ygPHeUwbkTYeohhFyYPJ6UmcPO1Fz4gJAEByAkgF81Enjqfl");
            sb.Append("BzXpj5fx+ixR9znLQ8erl/HH+bQZIxxblYeS4Wn8b3ImjAAAJKAIna/K5lcxetKJTq8Xx3sZPgqL");
            sb.Append("WEz/imX5EQAAuyWA8B/Z1SiedDrRO27jNCsAAJpMAKFU1HY8uQseezvaAQBA3QQQyhGPYppVQ1ax");
            sb.Append("AgBgbwkgB6yo8XjyxIgHAADpCCCHKMviqggex+dReV9AAACoQAA5MNl8FE96vTgXPAAAqIEAcjCy");
            sb.Append("mI+eRO/YdCsAAOojgByCrNjP49c4tqYuAAA1E0D23N2Uq2I/D+EDAID6CSB7LLsy5QoAgGYRQPZU");
            sb.Append("MfLRU2kOAEDDCCB7J4urstgcAACaRgDZJ1kW86tf41yxOQAADSWA7Itic8GXv8axaVcAADSYALIX");
            sb.Append("7sKHkQ8AAJpOAGk9064AAGgPAaTlsvlL064AAGgNAaSlFue96HQ6VrsCAKBVBBAAACCZzipXnnNP");
            sb.Append("81EnDDzcU78f/Z9/jp9/+ilOfvghoteL3voT3eh2s2LxrojlMpbxIW5u/oq3b9/GYpF6StkwZqtJ");
            sb.Append("DMqPAADYHQGkAgHk6/r9YZyensTTp708YHTLRyvIk0n26tdEu7kLIAAAqZiCxYb60R+OY7ZcRZFl");
            sb.Append("X7+exNnZYLPwUcj/fveHn8sPAADYFwIIlRQjHbPlMg8dr+P15CwGG+YNAAAOgwDCAxSjHbNYliMd");
            sb.Append("g01HOQAAODgCCPfQj+G4CB7FaMcgxA4AAKoSQPimuxGP1zEp6jrKxwAAoCoBhC9b13isjHgAALBV");
            sb.Append("Agj/0S+mW61rPMoHAABgSwQQPtGPcTHqYboVAAA7IoBwpz9e13qcSR4AAOyQAEKePZaxen1m1AMA");
            sb.Append("gJ0TQA5aP4azZbw27AEAQCICyMEq6j1ex0SlOQAACQkgB+kufBj4AAAgNQHk4BT7ewgfAADUQwA5");
            sb.Append("KEX4sL8HAAD1EUAOhvABAED9BJCDUNR8CB8AANRPANl7Cs4B+Lf5qBOdzobHaF5+NYCHEUD2WrHP");
            sb.Append("h/ABAEBzCCB7rAgfk0H5AQAANIAAsqf646XwAQBA4wgg+2g4i9f7MO9qMInVapXgmISsBgCQhgCy");
            sb.Append("b/rjWBr6AACgoQSQvTKM2euzUHMOAEBTCSB7o9zro/wIAACaSADZE5bbBQCgDQSQfTCcWfEKAIBW");
            sb.Append("EEDaTtE5AAAtIoC0Wj/G/1N0DgBAewggLabuAwCAthFA2krdBwAALSSAtFT/p155BgAA7SGAAAAA");
            sb.Append("yQggAABAMgIIAByYwWQVq9WGh0JEoCIBBAAASEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACS");
            sb.Append("EUAAAIBkBBAAACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAASEYAAQAAkhFAAACAZDqrXHnO");
            sb.Append("Pc1HnTielh/UpD9exuuzbvkR0ExZZPNl/Pn+Jt5dv4k3+SO3t7d3n/rM0dFR/v+P4/Hpj3Hy6FkM");
            sb.Append("Bq7vb9O2tJHn7f2VbXWTt9Wbb7RV3k5HRTs9ztvp5Fn08nZyhTefAFKBAAJ8VZbF/M+X8fv1NH+z");
            sb.Append("LB+r6mgYl789j2feUO9oW9rI8/b+snmM/7yJ64u8rcqHKslDyfC3/4vnru/mKgIIDzMbRhHaaj3y");
            sb.Append("AFJ+N7TZ8vLoi7/f7x3DWfkFalbtWjhaXaZ4+i4vV0df/Pe/c1Rs3OVythoefeHrbeU4Wg0vZ6vG");
            sb.Append("XPXadmea8P7y/WO4ashL0MN43jbWcna5s7Y6yn9/y3s3VP47+8LX+PbR0uuhZmpAADaRzWPU70Sv");
            sb.Append("dxzTTe9uftVtTC+Oo9cfxTgrHzoE2pY28ry9t2w+vmur44udtdXtNG+nXif6o3m4xJtDAAGoJIv5");
            sb.Append("qB+dnXYyPnM7jYvijXTve8raljbyvL23YqpV3la7DB6fWweRTj9Gc9d4EwggAA+VFXftenGcrJfx");
            sb.Append("b7cXvf3tKGtb2sjz9t6y+Sj6eUi7qKWtbmN63ItOfxxySL0EEIAHuHvzTHfX7mv2saOsbWkjz9v7");
            sb.Append("y8bFqMeGBebbcHsRx71+jKWQ2gggAPfUmDfP0u3FL3tTt6BtaSPP2/u6m57Wu2hKSxVu4+LYzYa6");
            sb.Append("CCAA97DuaDTqzbOQv4H+Mm59YaW2pY08b++rCB/1TU/7nvXIkQL15AQQgO9oZkejdHsRv7T4Dp62");
            sb.Append("pY08b+/vLnyUHzTUukC974ZDSgIIwLfMR83taJRuL17GvDxvFW1LG3ne3lsR1LYWPo6GMbycxWy5");
            sb.Append("jOVyVexj969jWTw+u4zLYbEzegV5cOv1f1/vuM7uCSAAX5ONo9/0W3dr0/i9bXfqtS1t5Hl7f1sJ");
            sb.Append("anmYGM5iWYSMxSQm54MYdLuR/+8/usXjg/M4nyxiUfz55WwdRh7k9rYx9Tz7TgAB+KIsxr9ctObN");
            sb.Append("6Pb6zxZNH9C2XzOY/Puu7m6PWQzLf5f78Ly9v3mMNgxqR8PLmC3zMDEZxBfyxvd1B+swslotY/bQ");
            sb.Append("IMLOCSAAX1DMW65+8664a5e/ec52NFXgS26v48+WJBBtSxt53t5XHtT6x7FJ/Di6XObB4zwGlZLH");
            sb.Append("57p5sM+DyDIP3HJIYwggAJ+bj6rNWz4arucnr1bFXbv8zTN/9/z+VIGi4zHcQqfjNq7b0EvWtrSR");
            sb.Append("5+29ZeNfNghqEcM8pC3Ot5I8/q07iMkiD3uXUkgTCCAA/3Lz8KkD607Gaj1HuZif/DBFx2MSiy3c");
            sb.Append("nWv+NCxtSxt53t7fPF5ukD6K8DHZzrDHV3XPi9GQy+2NNFGJAALwqen0AVMHjtZvmHedjPKhqtZ3");
            sb.Append("5zack3/7LpblaSNpW9rI8/be5qPqU6+KaVe7Dh9/655vJeBRnQACUMX6Dudiy2+YeYdjtkl34028");
            sb.Append("34fb9NqWNjr05202jt+rp4/4v11Mu/qWdcBbhhlZ9RBAAB5ovSzkNu5wfsng+QZviLfxruW36bUt");
            sb.Append("beR5GzF/WXWFsKO4/L/zaitdbawb50JILQQQgAe4W52l4rKQ95K/If5W/Y7nmxbfpte2tJHnbW6T");
            sb.Append("0Y/hb5F68OPfhJA6CCAA97TuaKR4pxycVJ73fdvS2/TaljbyvL2T/XldffTj+aA8r1MRQjast+FB");
            sb.Append("BBCAe0jW0VjrxY8HdDdO29JGnrcfbbDyVe2jH5/y2pCSAALwHWk7GoVuPDut+E745n2rlovVtrSR");
            sb.Append("5+0n5jeVV74anjRh9IM6CCAA35C+o3Gn++hxeba/tC1t5Hn7b/ObivHj6DIaMfuKWgggAF8znNXS");
            sb.Append("0TgI2pY28rz9zDwq54/TZ6ElD5cAAvAlR5exnNR4e673Y+ztdGRtSxt53v7XBtOvHj8SPw6ZAALw");
            sb.Append("H8OYLepal37faVvayPP2S7L3b8qzhxqG8o/DJoAA/MtRXC4nUft7Y/dR7F+lgraljTxvvyyLP6+r");
            sb.Append("rn51Un97UisBBOBTjVoWcs9oW9rI8/YrlvGuYv44+rFXnnGoBBAAAB4mex9VJ2Cp/0AAAQDgYZbv");
            sb.Append("Ku9+bgAEAQQAgAepXoD+OAyAIIAAAPAgy+oFIGEABAEEAIAHyKLyAAjkBBAAAB6g+gpY8fiR/VQQ");
            sb.Append("QAAAgHQEEAAA7m+DJXjtAUJBAAEAAJIRQAAAgGQEEAAAIBkBBACA+6u8CzrcEUAAAIBkBBAAACAZ");
            sb.Append("AQQAgPvr/RhH5SlUIYAAAJDE7btlecYhE0AAAIBkBBAAAO6v+ygel6cP9uZ9ZOUph0sAAQAgjdt3");
            sb.Append("YRIWAggAAA/Qix8rV6G/ifeGQA6eAAIAQCK3oQ4dAQQAgAfoxqPKRSBFGYghkEMngAAA8CC96nOw");
            sb.Append("mrkUb/ZnXN+W5+ycAAJUk43j92l5DsBB6W4yBDK9iXl52gxZjH+5CPkjHQEEqGAeo54Xa4CDNTiJ");
            sb.Append("YXn6cNO4aVACyca/xIU3tKQEEGiheufPZjHuH+dvHwAcrk1WwioGQRqSQOaj6EkfyQkgwIPMRz13");
            sb.Append("igAOXjeenW6UQBowDWseo2O30+oggEAL1VXAl4374bUagMJGdSAxjd/HRvMPlQACbfTmff7SmVYR");
            sb.Append("PgxTA/C3jepAIm4vXtY2CmI0v14CCNSo8t2j23eRcgwkM0cWgP8YxMkmCaSmUZD5qGM0v2YCCLRS");
            sb.Append("uhVE1uHDKzUAXzDYLIHE7cUvkS6DZHn4MJW4CQQQqFPvx6hawpdiBZH1tCuv1AB8zeB5XG5Qi55H");
            sb.Append("kLj4ZZxgWnFR89HLw4fR/CYQQKBO3UdRuYRvpyuI3N0lMu0KgG/bcDWswu1F9Ebz3YWQbB6jPHx4");
            sb.Append("S2sOAQRqtck66juaO5u/ULtLBMB9dc9/26gYfW16HL3+9kdCimnE/d5x3Oct7ehowyDFvQkgUKtu");
            sb.Append("bLKK4e1FL0ZbHAb5+ELtLhEA9zeI55vNw7pTjIT0RzHfQgrJ1qMenfU04nu9pR1dxm+n5Tk7J4BA");
            sb.Append("zXqbbCWbmx73Ny/ge+gL9dowhhvf8gJgH2xlFKRwO43jXif6xZSsCu9t2Xx89352z1GPO0dx+X/n");
            sb.Append("0Ss/YvcEEKhZ99lp5UL0O7dx8fHFunzk3orpVqN+dB70Qn1nOJvESXkOwKEbxGS2vbtSt8WUrPy9");
            sb.Append("rdMfxXhchJHsC+9x+WP54/N16MjfyzrFjbSLB7+fHV3+X5x3yw9IQgCBunWfxab1e4X1i3X+4tsv");
            sb.Append("Xqzndy/W/1G8gOefG49H0c//bBE8LqrUegxnMRmU5wBQGExiixnkzu00Li6KMNJbv8cVIeOfI38s");
            sb.Append("f/x4HToqvJcVji7j/6SP5AQQqN0WVhD5xG3xYn1892L97xfq/ChewPPPXVw8ZKrV54Yxkz4A+ILB");
            sb.Append("ZLadqVhJ3E29+hg/lu+qvzPyMAIINMDm07BSyV+sl5MQPwD4skFMlpeteE8bzhamXtVEAIEm6J7H");
            sb.Append("by24ZeTFGoDvyt/TFlufi7Vdw9nSVOIaCSDQEIPnzb5jdHTpxRqAexpMYrmNpXl34C58fH43LYv3");
            sb.Append("b8pTdk4AgaZo8ChIET4Whj4AeIDu+SKWjRoJOfpK+NjA0Y+W761AAIEGaWLxnvABQFXdwSRWjagJ");
            sb.Append("KWoYF9sNH1QmgECjNKl47+5OkfABwEaKmpDlLOqakXU0vIzlSg1jkwgg0DRl8V69IcSdIgC2qDuI");
            sb.Append("88VqPSUr2fvb0TAuixtpk3+W2v26ZVRahffxo3t8bT4ngEATDSaxqGkk5Gg4c6cIgJ0opmQtVsuY");
            sb.Append("Xe4wiJTBY7WYxLkbaY0kgEBTpR6yzl+wZ8viTtHA3RwAdqgbg/MiiBQjIpcxPNrGG93ReqrVbLlK");
            sb.Append("GjyOflSCXoUAAk326ZD1roLIJ3eKBl3RA4B0uoPzmCwWsSrCyHIWs8s8kAzzMJG/6X3tbW/9ufy9");
            sb.Append("a5gHjsvZLP97eehYLdZTrQx4tEMn/4WvynOg4bJsHi9/+T2mt1Umqn7qKIaXv8XzZ4OQOQA4eNk4");
            sb.Append("+r2LeOi763C2skdWBQIItFQRRpZ/3sTNuzfxptg8KQ8lX3rhLO4URTyOx49/jJOTZ9EbdE2xAoBP");
            sb.Append("VQogdwu2qJl8OAEEAIDDNh9F53hafnBfw5itJmEA5OHUgAAAcNCy98VUAlIRQAAAOGjLKpuADE+M");
            sb.Append("flQkgAAAcMCyqDIAYgne6gQQAAAOWLVd0B8/Un1elQACAMDhmt/EQ8vPixWwDIBUJ4AAAHCw5jcP");
            sb.Append("jx9xdBrPDIBUJoAAAHCYsnH8Xil/PLOn1gYEEAAADtL85cN3Py+mX50a/tiIAAIAwOGpOPoRw9/s");
            sb.Append("fr4hAQQAgAOTxfiXKqMfxfYfdv/YlAACAMBBmY96cVElfRxdxnP5Y2MCCAAAB2M+6sdxlalXueFv");
            sb.Append("54rPt0AAAQBga+ajTnT6/RjNs8jKx5ohK8NHlaGP3HAWE6MfWyGAAACwJVm8f5P/5/Y2pse96HU6");
            sb.Append("0e+PYpyHkVpl8xj1e9XDRxzFpblXWyOAAACwM7e307jIw0jnkzCSLI7kwWM86kendxyVs0duOFtY");
            sb.Append("+WqLOqtceQ4AABuYx6iTd/bLj77p6CiGp6dx8uhZ9AbdLdZW5AFn/me8/P1io9Dxt+EsVuZebZUA");
            sb.Append("AgDAdmTj6PeqLW9bTHM6Onocjx//GCcnjyJ6vegVD3e/Hk6yrBhLWcZy+T5ubt7FmzfTYvbX9hxd");
            sb.Append("xnKh8HzbBBAAALZjPopO1SWmmuZoGLPFJIx9bJ8aEAAA+JTwsVMCCAAAW5Gtl8BqufW0K+FjlwQQ");
            sb.Append("AAC2YvlumwUY6R0NZ2o+EhBAAADYgnIPkFY6isvZMhaTgfCRgAACAMAWLKN9AyBHMbycxXK1iPOB");
            sb.Append("6JGKAAIAwBYM4vlyFpfDYd6tb7p/gsfk3KhHapbhBQBgB4oNAZfx581NXG97f46Kjo6Gcfrbc6Md");
            sb.Append("NRNAAABIotg4cLn8czebBn5Jsdv649M4OXkWA6GjMQQQAABqlOXBJPJgsoz372/i3bvIw8nHavbb");
            sb.Append("74SUYvf0u7PHjx9H/PhjnDx6FL1eL7pdgaOpBBAAACAZRegAAEAyAggAAJCMAAIAACQjgAAAAMkI");
            sb.Append("IAAAQDICCAAAkIwAAgAAJCOAAAAAyQggAABAMgIIAACQjAACAAAkI4AAAADJCCAAAEAyAggAAJCM");
            sb.Append("AAIAACQjgAAAAMkIIAAAQDICCAAAkIwAAgAAJCOAAAAAyQggAABAMgIIAACQjAACAAAkI4AAAADJ");
            sb.Append("CCAAAEAyAggAAJCMAAIAACQjgAAAAMkIIAAAQDICCAAAkIwAAgAAJCOAAAAAyQggAABAMgIIAACQ");
            sb.Append("jAACAAAkI4AAAADJCCAAAEAyAggAAJCMAAIAACQjgAAAAMkIIAAAQDICCAAAkIwAAgAAJCOAAAAA");
            sb.Append("yQggAABAMgIIAACQjAACAAAkI4AAAADJCCAAAEAyAggAAJCMAAIAACQjgAAAAMkIIAAAQDICCAAA");
            sb.Append("kIwAAgAAJCOAAAAAyQggAABAMgIIAACQjAACAAAkI4AAAADJCCAAAEAyAggAAJCMAAIAACQjgAAA");
            sb.Append("AMkIIAAAQDICCAAAkIwAAgAAJCOAAAAAyQggAABAMgIIAACQjAACAAAkI4AAAADJCCAAAEAyAggA");
            sb.Append("AJCMAAIAACQjgAAAAMkIIAAAQDICCAAAkIwAAgAAJCOAAAAAyQggAABAMgIIAACQjAACAAAkI4AA");
            sb.Append("AADJCCAAAEAyAggAAJCMAAIAACQjgAAAAMkIIAAAQDICCAAAkIwAAgAAJCOAAAAAyQggAABAMgII");
            sb.Append("AACQjAACAAAkI4AAAADJCCAAAEAyAggAAJCMAAIAACQjgAAAAMkIIAAAQDICCAAAkIwAAgAAJCOA");
            sb.Append("AAAAyQggAABAMgIIAACQjAACAAAkI4AAAADJCCAAAEAyLQsgWVw96USnU/fxJK6y8lsigfp+70/8");
            sb.Append("ogEAtqplAaQbP/xcnnI4sldxvSjPE/v5h255BgDANrRuCtbgZFie1WkR5y/n5Tm7lr26zlu8Bv1x");
            sb.Append("PB+U5wAAbEX7akAGJ9GECBLTP0zDSiG7il/P6xn+6J8+DeMfAADb1cIi9EE8H/fL8zot4vqVBLJz");
            sb.Append("y7/qGf3IY+6LM/EDAGDbWhhAIrpPT6MREeT8V6MgO5XF1R/T8jyx4UkedQEA2LZWBpDonsWLRszD");
            sb.Append("WsRfy/KU7aut+LwfY8UfAAA70c4AkmtGMXrE9EYx+q7UV3x+Gk/NvgIA2InWBpAYPI9GlIIoRt+N");
            sb.Append("GovPhy/OFJ8DAOxIewNI3kV8eqoYfW/VWHx+YvYVAMDOtDiAKEbfX/UVn/fHzxWfAwDsUKsDiGL0");
            sb.Append("PVVj8fmp4g8AgJ1qdwDJKUbfP7UVnw9fhK0/AAB2q/UBRDH6nqmz+FzxBwDAzrU/gChG3y91FZ/3");
            sb.Append("x2HrDwCA3duDAFKUgryIJkzEUoy+oWweo7qKz0+fWnoXACCBvQggEYNoRimIYvTNfIi39RR/xAvF");
            sb.Append("HwAASexJAClKQcaNWJJ3+sdVGASpZv7yvKbi8xNL7wIAJLI3ASS6T6MZpSDXoRSkguwq6pl91Y+x");
            sb.Append("4g8AgGT2J4AoRm+12pbe7Z+GrT8AANLprHLl+R6Yx6hzHPWUMX+qH+Pla3tK3Fd2FU969Uy/Gs5W");
            sb.Append("MTEAAgCQzB6NgBQUo7dSXUvvxjBs/QEAkNaeBRDF6O2TxVVdS++Onys+BwBIbO8CiGL0lslexXU9");
            sb.Append("xR9xqvgDACC5/Qsg0Y2zF43YllAx+j3UVnw+fKFGBwCgBnsYQHKDEzujt0F2Fb+e11T9ofgDAKAW");
            sb.Append("+xlAYhDPx81Yklcx+jfUVXzeH4etPwAA6rGnAaQoBTlVjN5k2TxGdRWfnz4Ns68AAOqxtwFEMXrT");
            sb.Append("fYi39RR/xAvFHwAAtdnfAKIYvdHmL+vZeDCGJ5beBQCo0R4HkJxi9GbKrqKe2Vf9GCv+AACo1X4H");
            sb.Append("EMXojVTb0rv907D1BwBAvfY8gChGb5w6l959cab4HACgZnsfQKJ7Fs0oBVGMvlbX0rsxDFt/AADU");
            sb.Append("b/8DSG5wohi9GbK4qmvp3fFzxecAAA1wEAFEMXpDZK/iup7ijzhV/AEA0AiHEUAUozdCbcXnwxdh");
            sb.Append("6w8AgGY4kACiGL12dRafK/4AAGiMgwkgitFrVlfxeX8ctv4AAGiOwwkgOcXoNcnmMaqr+Pz0qaV3");
            sb.Append("AQAa5KACSAyeRxNKQQ6vGP1DvK2n+CNeKP5Ym4860elseIzm5VcDAKjusAJIdOPpqWL01OYvz2sq");
            sb.Append("Pj+x9C4AQMMcWAApSkFeNGJJ3ukfo5gfwihIdhX1zL7qx1jxBwBA4xxcACmW5G1GKcjb+FCe7rPa");
            sb.Append("lt7tn4atPwAAmucAA4hi9GTqXHr3xZnicwCABjrIAKIYPZG6lt6NYdj6AwCgmQ4zgChGTyCLq7qW");
            sb.Append("3h0/V3wOANBQBxpAFKPvXPYqrusp/ohTxR8AAI11sAFEMfpu1VZ8PnwRtv4AAGiuAw4gRSnIOOqf");
            sb.Append("iLWHxeh1Fp8r/gAAaLSDDiDRfRpNKAXZu2L0uorP++Ow9QcAQLMddgCJbpy9aMaSvHtTjJ7NY1RT");
            sb.Append("8bmldwEAmu/AA0hucKIYfas+xNt6ij8svQsA0AICiGL0rZq/PK9l+pWldwEA2kEAyTWlGP385bw8");
            sb.Append("b6nsKuqZfWXpXQCAthBACg0pRo/pH60uRrf0LgAA3yOArClG35ildwEAuAcB5CPF6Jux9G6jDSar");
            sb.Append("WK02PCYaGgDYnADyt0E8HzdhU5A2FqNncVXT0rv906eW3gUAaBEB5BPdp6eK0avIXsV1TUvvvlD8");
            sb.Append("AQDQKgLIpxSjV1Jf8fmJpXcBAFpGAPmX5hSjX79qSQKprfi8H2PFHwAArSOAfK4hxeiL65ftKEav");
            sb.Append("rfj8NGz9AQDQPgLIfyhGv7/6is+HL84UnwMAtJAA8gWK0e+pxuJzW38AALSTAPIl3bNoRClIw4vR");
            sb.Append("6yo+74+fKz4HAGgpAeQrBieK0b+pxuLzU8UfAACtJYB8zeB5NKIU5Lyhxeh1FZ8PX4StPwAA2ksA");
            sb.Append("+apuPG3EpiANLEbP5jGqq/hc8QcAQKsJIN+gGP1rPsTbeoo/wtYfAADtJoB8i2L0L5q/PK+n+Pz0");
            sb.Append("qaV3AQBaTgD5DsXon8muop7ZV8N4ofgDAKD1BJDvaUwx+q+NGAWpa+ndGJ5YehcAYA90VrnynK/I");
            sb.Append("rp5Er5YlZ/9tOFvFpM5eeHYVT3p1TL/qx3j52upXNFKWzWO5/BA3N3/F27dv148tFl+5Svr9u7qy");
            sb.Append("n3+On+OnODn5IXq9QXQ9tyvIIpsv49WHm/jr+m0ULf+1du/n7Z43evx8mrf5D09jMNDg36Zt7yXL");
            sb.Append("2ymW+fUf8eEmb6v8oY+vAcXMha+9DKx9fC0o2u7niJ9OTuKHXi96+YuBZyeHQAC5l3mMOsdRz7pP");
            sb.Append("nxjOYlVnApmPonNcQyv0x7F8feZFmWbIOx3zVy/jj+vptzsYD9TvD+P09CSenuWBpHyMz2yz7fP2");
            sb.Append("Hr94Hk/zDrP2zmnbb8ry9lkuX8XNzXUeMr4TLjaWh5PhaZyePI0zYZl9VQQQvm82jCKo1Xz0V+Nl");
            sb.Append("+Q0lt1yN+1/6nnZ/DGflt7CHmvG8+t4xXO3xr+CelqvlbLjqp7oG+nmbL2u72L9vOV71v/R9f++o");
            sb.Append("eDEvl7PVcGdt318Nx7P8N9wQ2rYRinYZjxNe8984+sNx/npQfmOwJ9SA3NPg+bgRS/LWVoyevYrr");
            sb.Append("nd7x+Zph2PqD+mQxHz2JJ51e9I63O+LxTYtpHPd60XkyiqtG7kSaSLHn0JNO9HrHMd1Z2y9ien4c");
            sb.Append("vaKtD6mpte1n8mv96mrdJp3OXbucnye85r9hMT3PXw868WR01cyNiaECAeS+uk+jCfsS1lWMXlfx");
            sb.Append("eX/8XPE5NSg6I6N18DjOe2e19UHyIHJ+3DvAjsdd8OvstHP8maKti07e3qcQbfu3PIRdra/zInTk");
            sb.Append("1/r5ebo2qeAuiDyJ0UElZfaVAHJvTdkZfRF/LcvTVLKr+LWWIvx+nD41/5XE1neGi87ItL7g8Zm/");
            sb.Append("Ox6HkELy15t1+9fUE1yc54FvXzt42vYfRU3jx1GO8qF2KEaVipsS8zxKQnsJIA/QPXsRjdiX8Cbx");
            sb.Append("C8/yr3peoIcvrHxFUllxNzTlneEHyTsexWjIHt/9zPJOYbHSXt3tv48hRNvul8X0OHp5CIG2EkAe");
            sb.Append("ZBCN2Jdw+kckKwUp7gbXs/NgDBV/kMzdtJReC+6G7msHbr3ceVFnU35ct6bsvbQN2nZP5SFEmKOt");
            sb.Append("BJAHOrxi9A/xto53rf44nssfJJHFVY3TUqrYtxDSlL2W/m0R579etX6ai7bdb8IcbSWAPNSBFaPP");
            sb.Append("X9ax8WCeP06fWpufBO7qPRrXP7uHIoTswwyMZnaQS4vz+LXFvTttewiEOdpJAHmwbpy9aMI8rATF");
            sb.Append("6NlV1DP7ahgvFH+wc0X42FK9R38Yw/EsZstiV+RVsb/Sv4/14/nnx+MYrneO3o7p8ZN23/2cj5rb");
            sb.Append("QS4tzl/mz5QW0rY7V+wCPxyOYzz7eO3nx+fX/t9H+fn8z46Hw2Ij9O3Jw9xL5SC0TX5h8GCzVR5B");
            sb.Append("vrhhUNKjP97pBk/Lcf/L/+6uj33eefAzNiKsyzY21txsk7XlbLydDeB2/DrwRdvYLK/q16jh6Kfc");
            sb.Append("AVbbpjEbfvH7+dZRbAg4ni1X29sjdLmaFZsdfuHfevBRx+sAbMAISCUNKUZfXO+uGL3GpXfHB1T8");
            sb.Append("MZh86U7Zro5ZI1Zxa4L5aLNpV/3hLJar1zE5G1SeKtgdnMXk9SqWeQrNOyDVtfLuZxZXv9YzvbOK");
            sb.Append("xfWrFk1x0bbb04/heBx55li/hr6enMXZoBvdrQ3Qd2NwNonXq2XMhhu9Cuy2PwA7IIBUtPfF6HUt");
            sb.Append("vds/DVt/sEvFvPjjylML84A8W+YdkerB43PdQd4BWW4WDqfHo1ZNZdksAPbzAJh3CvPfw5emu91N");
            sb.Append("cxnHOO/Qbe01ukWdO227uXUbrH/+4ibDWeSZY8fyIDJ5HcvxJq2acnEa2IL8RYVKtjGFYxtHf7X9");
            sb.Append("Eez6frYDmn1Vg6pTB/doCtZGU1PydtjpHIfNpna2ZppQhakv66NftP9Df8blapn/e9V/5/8cydpX");
            sb.Append("26bxn7bq5+8/+c9UfrouG019Ng2LFjECUtkeF6Nnr+K6luGPYdj6g92Zx6hXdWrKMGbLyY7vhA5i");
            sb.Append("sqw+stqOaUI3MXro8FO/aPtVrF4X7f/QX0D3nxGmqg1ban77attK8jYoRjXXox3F9Kry4bp0z/4X");
            sb.Append("lQdCFn/FrtemgW0RQDYxOGnEnPrpH9tdgi97dV3L9Kv++HneBYPdmI+Oo9rMq36Mdx4+St2zeD2r");
            sb.Append("+KrShlqQ6fQBv4N+DIuO4bpzXD5UVTcPd683rIFqeudO2z7QT3fBI2+Doq6jOTa5ufk2PpiFRUsI");
            sb.Append("IBsZxPON5mxuyTbn0NZYfH6q+INd2WBJ6f74f5F0VejB88p3QKc3e7IW5/rO/N0d6e3JO8pVw93a");
            sb.Append("nnTutO2dwV1BeSNVvrmZYHl+2BIBZEPdp6eVp0xszxaLz+oqPh++SNvJ44BssCpQfxz/S/7E3OAO");
            sb.Append("6PSm1fsqFNYrjG3jzvyXbBDu9qFzp23bovpKm28NgdASAsimumfRhFKQreyMns1jVM/OgzFU/MGu");
            sb.Append("zF9WXBWoH+P/ndUzJ7xyZ24abR4E6Y+3u8LYf21Wu9fmzp22bZdBxQSykORoCQFkC6q+UGzXNu4g");
            sb.Append("fYi39RR/xAFt/UFSWVxVDdW1jsp14+lptdvJbZ2Gte4gp2jwDWr32tq507Yt1Pup2uyKtx8avlgC");
            sb.Append("3BFAtmFPitHnL+vZvKp/+nSHd+U4aJuMftSciitP72zhNKxkHeS1XvxUeapQ+2jblur+ED+Xp7CP");
            sb.Append("BJCt2INi9A2KdDczjBf13WZmz81v2jj6Ueo+jWqDIO0qlk7bQS5UH11q291lbXuALMVLSwggW9L2");
            sb.Append("YvS6lt6N4Uke32AX5lE5fzSiJqkbP1S6BdqeHZHTd5DvdKs1bKto27YzmsR+E0C2pc3F6DUuvVv3");
            sb.Append("NBf2V3b1R8V9P5qzIeZeF6IOZ7V0kA+CtgUaTgDZotYWo9e19G7/NGz9wW5k8arqdv77MCrX9Oks");
            sb.Append("/XEsJzW2ctUC3zbQtkALCCDbtNE66NvzsGL0DVYJ2tDwRU1LnLL/sldRPX80KH5U7cw1eh74MGav");
            sb.Append("Xfu7oW2BdhBAtmqDArxtekgx+gYdtc00Z5oLe6jyqF4/fuqVp01QeSWcphai92O8nNQ/wrSXKwxp");
            sb.Append("W6A9BJAt6569yLvWdbt/EWpdxef98XPF5+xM5dWv8q7TD3tx+7ihO0s3YXWxfaVtgRYRQLZuEE0o");
            sb.Append("BblXMXqNxeenij/YmSw+vC1PH6px9R9WwgFg/wggO9CaYvS6is/dqWOnlvFXxSd2v1Hzrzbztk2b");
            sb.Append("gQBwUASQXWhDMXo2j1FdxeeKP9il7ENUHQDZJ61YiheAgySA7EQbitHzTlo9xR9h6w92aoORvZ/3");
            sb.Append("owAEqEOW5f+bx3x+FVejUYxGT+LJk/LodKLzoKMXtcyQhkQEkB1pejH6/OV5LdOv+qdPLRHJTmWV");
            sb.Append("C0AatgLWppq+Fwi0XBE2rq5G64CxDg29XvR6x3F8fB7n02lMp4tYLMqj/DvAHQFkZxpcjJ5dRT2z");
            sb.Append("r4bxQvEHO7asWgCSdxHOe5/fhaz7cBcUmiOL+Tpw3F2fRdg4P5+uAwbwMALIDg2ej6P+iVj/LUav");
            sb.Append("a+ndvdhhmobbYAUsgC/I5lcxWoeOXhyvA0f5CaAyAWSXuk+jCaUg0z9GMf84ClLj0rtjxR8AtEI5");
            sb.Append("2lGMdByfx1TogK0SQHaqKcXob+NDeVrb0rv907D1B7tXfQnevbP4K28N4GE+Bo9ytKN8FNguAWTH");
            sb.Append("GlOM/nIeWZbFVV1L7744U3wOQHNlxVQrwQNSEEB2riHF6NPj6PXqKmgdhq0/AGimLOajJ9HpmWoF");
            sb.Append("qQggCTSjGL0+/fFzxecANE+xKW8x6iF5QFICSAoNKUavRz9OFX8A0DTZVTzpHe9u1KPfj/5wGMPh");
            sb.Append("OGazWX4sY7ksj9UqVt88ljE+5DuX7D0BJIlunL1owDysOgxfhK0/AGiUdfjY7oa8/f4wxnnQWC7L");
            sb.Append("EPH6dbyeTGIyOYvBYJAf3eh2y6P8O3CoBJBUBicNKEZPb6j4A4Am2WL46BejG8vlOnC8fj2Jszxo");
            sb.Append("5PkC+A4BJJlBPD+08dT+OGz9QVq9+KnqZZY/X78/LaJNx0TtFfzHPEYbh49+DMez9evF62J0Q+KA");
            sb.Append("BxNAEuo+PT2oYvT+6VPDzAA0RBZXT45jk8Xo70Y8XsfkbOD9DTYggKR0UMXow3ih+IM2sXEf7LXs");
            sb.Append("6tcNlqLvx3C2LEc8yoeAygSQpA6oGH14YvoHNejGDz+XpwAfZVfxa+X00Y9xMeohecDWCCCpHUQx");
            sb.Append("ev5irfiDmvQqF4G8jQ9ZeQrslfnL6nUfw9lrqznClgkgyR1AMXr/NGz9QV26lYdAFvGXOViwf7Kr");
            sb.Append("+KNi4Ud/vIyJ+2mwdQJIDfa9GH344kxxHvXp/VT5+prezMszYF9kr64rjn6oZYRdEUDq0D2L/S0F");
            sb.Append("GYatP6hV94eoXAby9kOYhQX7JItX19XiR3/8XC0j7IgAUpPByX4mEC/Y1G+DvUAW1/FKAoH9kb2K");
            sb.Append("avmjH6fmEsPOCCB1GTyP/SsF8YJNE2yyEpY6ENgry7+qTb9Sywg7JYDUphtP921TkOELK4XQCJuM");
            sb.Append("MKoDgf0xv6lYff7zD2oZYYcEkBrtWzH6UPEHTbHJctfTP+LKNCzYA1l8eFuePlD/p155BuyCAFKn");
            sb.Append("fSpG74/D1h80xyCqD4Is4lohCADsjABSs30pRu+fPjVcTaNscm0tzl+GiVjQdsv4q+rug8BOCSB1");
            sb.Append("24tidGul00CbTMOKafxhHhYA7IQAUrs9KEYfnlh6lwYaxPMN0v3i/Fe1IHCgFpbDg50SQBqge/Zi");
            sb.Append("gzu1devHWPEHDbXZQg+LOP/1ysaE0Fob7AnU2k1J38YHL1q0gADSCJsUzNbMWuk02aYLPSzOozdS");
            sb.Append("DQIHZ/FX1DoGUnkDRWgHAaQh2lqMPnxxpvicRhs8H2+23PX0OJ6YiwUttMmmpNOoa0ug7OpJdHrn");
            sb.Append("1TZQhJYQQJqilcXow7D1B43XPYv/bXhxLc57Qgi0UK/yHKxiS6DUUzCzmI+eRO98k+ixCOUrtIEA");
            sb.Append("0hjtK0bvj58rPqcVumf/2zjgr0PIaN6qeeGZzMSB26gObHEev6a68ZBdxehJL46nm497vFUEQgsI");
            sb.Append("IA3SrmL0fpwq/qA1unH2vw2nYuUW0+PoPRnFvMnv79k8rkZP4kmnE7/aUJFD130am9zb2/1qeHej");
            sb.Append("HsWUqy1kjzUreNEGAkijtKgYffgibP1Bq2xhKtbaYhrHvU6zRkOyvBNzNVqHjk7vOM7znoz541DY");
            sb.Append("dHbBIs57T3YSQrJ5fs1uadTjX6Y3NlKl8QSQhtm4YDaRoeIPWqh79jpmWwr569GQvMP/ZHRVy4hI");
            sb.Append("9slIR6eXd2LOp0IHfMHmswvuQshoSxd6Ni+mW3Wid5xfszu5aG2kSvMJIE2z4XBxEv1x2PqDthpM");
            sb.Append("llsLIYXF9Hw9ItJ5kndQruY7qrvI8k5LHjiKUY6849LJQ0fPSAfc02abkt5ZxPS4l19/Vadg/jNK");
            sb.Append("2Tve3nSrrylq1rYVmGAXBJDG6cbZRhsX7F7/9Gn+XUJbddchZOurzi3yDsr5cR4M7gLCkyKQjK7i");
            sb.Append("Kg8O8zyVZMVR/tEv+vhnij9f3CEdFWHjyfprdTq9vNOSB45ilEPigAfbVo3lopyCeXfDoRj9LK7b");
            sb.Append("8pOf+HgtX62v47truNooZT/6FV+r/glMn7/2FN9z/jqTB6IrIYW6rGig2Sp/oVwVv57mHcP8u6Od");
            sb.Append("qj6v9vV3vlzNhl/6effn6I+X5c+6ZcvxKu8TffHf/OYxbMozqeK10B/nz5od07a7Mxt++d9u6tHP");
            sb.Append("X3uLRtnl992Y5w2HxghIIzW4GH14Yuld9kQxErKK5daHQoBGGkxac733h+NYvp7EoJhusMt9wt5+");
            sb.Append("+PbILOyIANJQzSxG78dY8Qd7pihMX87asfgDsJltLkSxG/0YzpbxenL2yVTnHU7NXvwVFu2lDgJI");
            sb.Append("UzWxGL1/Grb+YB91B2fxerWM8VAMgX1XjHzOmnit94cxW76OyXrY4zM7GwV5G/YtpA4CSGM1rxh9");
            sb.Append("+OLTOzKwb/JrbnI3GiKHwH4bFNd6Y6Zj3Y16rD5Oufqi7Wym+l+LsG8hdRBAmmxw0qCd0Ydh6w8O");
            sb.Append("QTEaMnm9yoPIsPLqM03Q7w/j1JAlfFUxHWu1rHP6ZT/6w1ksV18Z9fhc9yxeN3v+GNybANJo21i7");
            sb.Append("fDv64+eKzzko3cEkXq+DSFtGRIrOzDhmy2WxumH+vU/iTP6Abys69atib6C0F/k6eCxfx+vJ4GEz");
            sb.Append("C4pC+i2HkLfmYFEDAaThuk9Pa7w781HfnVQO1scRkVXRSRk3aVTkLnCMZ8tY5oFjtSo6M2cx6LpW");
            sb.Append("4WGKFfGK0ZDZbuvA8heP4bgY8VjdBY+Kl2pxc2SbIzcLc7CoQadYi7c8B+Beik3GlvHq5iau36bY");
            sb.Append("HLDYjOzn+Pn0JE6e9qKX91zEDNiVYtfyl/HH9Tau7SJ0nObXbXFzoHxoa/Lvc/Rr/DFdPHiDw/UU");
            sb.Append("zRfP42n+TXktoQ4CCMA2FLsNL5ex/PAhbv76K+Lt23i7/kTeOfhm7+CTnY5/zkNG/p+fTk7ih8iD");
            sb.Append("Ri/vHOgdQK2KXcNfvbqJv/7Kr+nios4v6C9d0v31hVzcKPgpTn54Gr2EnftsfhWvbv6K6/wbXHz2");
            sb.Append("grP+vvLXltOfTuLp0+ojL7BNAggAAJCMGhAAACAZAQQAAEhGAAEAAJIRQAAAgGQEEAAAIBkBBAAA");
            sb.Append("SEYAAQAAkhFAAACAZAQQAAAgGQEEAABIRgABAACSEUAAAIBkBBAAACAZAQQAAEhGAAEAAJIRQAAA");
            sb.Append("gGQEEAAAIBkBBAAASEYAAQAAEon4/9oPskPQgAEHAAAAAElFTkSuQmCC");
            return sb.ToString();
        }

        public NullScraper() : base(NullLogger.Instance)
        {
        }

        public override bool IsNull => true;

        ///// <summary>
        ///// https://stackoverflow.com/questions/2070365/how-to-generate-an-image-from-text-on-fly-at-runtime
        ///// </summary>
        ///// <param name="text"></param>
        ///// <param name="font"></param>
        ///// <param name="textColor"></param>
        ///// <param name="backColor"></param>
        ///// <returns></returns>
        //private static Image DrawText(string text, Font font, Color textColor, Color backColor)
        //{
        //    //first, create a dummy bitmap just to get a graphics object
        //    Image img = new Bitmap(1, 1);
        //    Graphics drawing = Graphics.FromImage(img);

        //    //measure the string to see how big the image needs to be
        //    SizeF textSize = drawing.MeasureString(text, font);

        //    //free up the dummy image and old graphics object
        //    img.Dispose();
        //    drawing.Dispose();

        //    //create a new image of the right size
        //    img = new Bitmap((int)textSize.Width, (int)textSize.Height);

        //    drawing = Graphics.FromImage(img);

        //    //paint the background
        //    drawing.Clear(backColor);

        //    //create a brush for the text
        //    Brush textBrush = new SolidBrush(textColor);

        //    drawing.DrawString(text, font, textBrush, 0, 0);

        //    drawing.Save();

        //    textBrush.Dispose();
        //    drawing.Dispose();

        //    return img;

        //}
        public override Task<byte[]> ScreenshotAsync(Uri requestUri, System.Drawing.Size size)
        {
            return Task.FromResult(System.Convert.FromBase64String(GetLogoPngAsBase64()));
            //var font = new Font(familyName: "Times New Roman", 14);
            //var img = DrawText("holla", font, Color.Black, Color.White);
            //using (var ms = new MemoryStream())
            //{
            //    img.Save(ms, ImageFormat.Png);
            //    return Task.FromResult(ms.ToArray());
            //}
        }
    }
}
