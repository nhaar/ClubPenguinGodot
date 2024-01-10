using Godot;
using System;
using static ThinIceGame;

namespace ClubPenguinPlus.ThinIce
{
    /// <summary>   
    /// Class for all the levels in Thin Ice
    /// </summary>
    public static class ThinIceLevels
    {

        public static readonly Level Level1 = new(@"
map
15wall
1wall,goal,12ice,wall
15wall;
origin(1,9)
puffle(14,10)
");
        
        public static readonly Level Level2 = new(@"
map
12empty,3wall
12empty,wall,goal,wall
12empty,wall,ice,wall
7wall,empty,5wall,ice,wall
wall,5ice,wall,empty,wall,5ice,wall
5wall,ice,3wall,ice,5wall
4empty,wall,5ice,wall,4empty
4empty,7wall,4empty;
origin(1,6)
puffle(2,10)
");
        
        public static readonly Level Level3 = new(@"
map
3wall,8empty,3wall
wall,goal,wall,8empty,wall,ice,wall
wall,ice,5wall,4empty,wall,ice,wall
wall,ice,2wall,2ice,6wall,ice,wall
wall,12ice,wall
wall,2ice,4wall,2ice,2wall,2ice,wall
4wall,2empty,8wall;
origin(2,6)
puffle(14,7)
");
        
        public static readonly Level Level4 = new(@"
map
5wall,6empty,5wall
wall,3ice,8wall,3ice,wall
wall,3ice,2wall,4ice,2wall,3ice,wall
2wall,12ice,2wall
empty,wall,ice,4wall,2ice,4wall,ice,wall,empty
empty,wall,ice,4wall,2ice,4wall,ice,wall,empty
empty,wall,ice,wall,empty,wall,4ice,wall,empty,wall,ice,wall,empty
empty,wall,ice,wall,empty,wall,4ice,wall,empty,wall,goal,wall,empty
empty,3wall,empty,6wall,empty,3wall,empty;
origin(1,1)
puffle(3,8)
");

        public static readonly Level Level5 = new(@"
map
12empty,3wall,2empty
12empty,wall,ice,wall,2empty
3empty,10wall,ice,3wall
4wall,12ice,wall
wall,goal,14ice,wall
3wall,13ice,wall
2empty,15wall;
origin(1,6)
puffle(14,7)
");

        public static readonly Level Level6 = new(@"
map
12empty,3wall,2empty
5empty,5wall,2empty,wall,goal,wall,2empty
2empty,4wall,3ice,4wall,ice,3wall
3wall,9ice,wall,3ice,wall
wall,11ice,wall,3ice,wall
4wall,3ice,wall,8ice,wall
3empty,14wall;
origin(1,6)
puffle(2,10)
");

        public static readonly Level Level7 = new(@"
map
15wall,3empty
wall,13ice,wall,3empty
wall,ice,11wall,ice,4wall
wall,ice,3wall,goal,7ice,thick,3ice,wall
wall,ice,11wall,ice,2wall,ice,wall
wall,ice,11wall,ice,2wall,ice,wall
wall,ice,14wall,ice,wall
wall,ice,4wall,2ice,2wall,4ice,2wall,ice,wall
wall,16ice,wall
5wall,6ice,7wall
4empty,8wall,6empty;
origin(1,2)
puffle(14,7)
");

        public static readonly Level Level8 = new(@"
map
5wall,3empty,5wall,4empty
wall,3ice,5wall,3ice,wall,4empty
wall,ice,wall,ice,2wall,ice,2wall,ice,wall,ice,wall,4empty
wall,2ice,thick,2ice,thick,2ice,thick,2ice,4wall,empty
3wall,ice,2wall,ice,2wall,ice,4wall,goal,wall,empty
empty,2wall,ice,2wall,ice,2wall,ice,4wall,ice,wall,empty
empty,wall,4ice,thick,2ice,thick,2ice,wall,2ice,2wall
empty,wall,3ice,wall,ice,2wall,ice,wall,ice,wall,3ice,wall
empty,wall,5ice,2wall,3ice,2wall,2ice,wall
empty,wall,2ice,10wall,2ice,wall
empty,2wall,13ice,wall
2empty,15wall;
origin(0,3)
puffle(6,5)
");

        public static readonly Level Level9 = new(@"
map
16wall,empty
wall,14ice,2wall
wall,ice,4wall,ice,thick,6ice,thick,ice,wall
wall,ice,thick,13ice,wall
wall,ice,thick,5wall,ice,4wall,ice,3wall
wall,ice,thick,5wall,goal,wall,2ice,wall,ice,wall,2empty
wall,ice,thick,7wall,2ice,wall,ice,wall,2empty
wall,2ice,4wall,5ice,wall,ice,wall,2empty
wall,3ice,2thick,2ice,3wall,ice,3wall,2empty
wall,ice,wall,5ice,3wall,ice,wall,4empty
wall,3ice,3wall,5ice,wall,4empty
5wall,empty,7wall,4empty;
origin(1,0)
puffle(14,7)
");

        public static readonly Level Level10 = new(@"
map
19wall
wall,17ice,wall
wall,3ice,8thick,4ice,thick,ice,wall
wall,15ice,thick,ice,wall
wall,6ice,6wall,3ice,thick,ice,wall
wall,6ice,2wall,ice,2wall,2thick,2ice,thick,ice,wall
wall,6ice,2wall,ice,2wall,2thick,4ice,wall
wall,17ice,wall
wall,17ice,wall
wall,6ice,3wall,4ice,3wall,ice,wall
wall,9ice,4thick,lock,ice,wall,ice,wall
wall,2ice,2wall,2ice,3wall,4ice,wall,goal,wall,ice,wall
wall,2ice,2wall,2thick,2ice,wall,4ice,3wall,ice,wall
wall,17ice,wall
19wall;
puffle(9,5)
keys(1,1)
");

        public static readonly Level Level11 = new(@"
map
13empty,3wall,2empty
13empty,wall,goal,wall,2empty
11empty,3wall,ice,3wall
empty,11wall,2ice,thick,2ice,wall
empty,wall,8ice,2wall,thick,wall,ice,wall,thick,wall
empty,wall,3ice,4wall,2ice,wall,thick,ice,thick,ice,thick,wall
empty,wall,4ice,thick,ice,wall,2ice,3wall,ice,3wall
3wall,thick,ice,wall,2ice,wall,2ice,thick,ice,wall,ice,3wall
wall,2ice,thick,3wall,ice,3wall,ice,2wall,lock,3wall
wall,ice,wall,3ice,wall,thick,4ice,wall,ice,3thick,wall
wall,ice,wall,ice,wall,ice,wall,thick,wall,3ice,wall,ice,2wall,thick,wall
wall,2ice,thick,wall,ice,wall,ice,wall,3ice,wall,2ice,wall,thick,wall
3wall,2ice,2thick,ice,wall,3ice,4wall,thick,wall
2empty,wall,2ice,wall,2ice,wall,2ice,6thick,wall
2empty,16wall;
origin(1,0)
puffle(15,11)
keys(6,9)
");

        public static readonly Level Level12 = new(@"
map
6empty,4wall,empty,6wall,2empty
6empty,wall,2ice,3wall,2ice,wall,ice,wall,2empty
6empty,wall,ice,thick,3ice,thick,ice,wall,ice,wall,2empty
5empty,3wall,ice,3wall,ice,2wall,ice,2wall,empty
6wall,2ice,thick,3wall,3ice,thick,ice,wall,empty
wall,2ice,3wall,ice,wall,ice,wall,ice,wall,ice,2wall,ice,2wall,empty
wall,ice,thick,3ice,thick,ice,thick,ice,thick,wall,thick,2ice,thick,wall,2empty
wall,ice,thick,4ice,wall,ice,wall,ice,wall,ice,2wall,ice,wall,2empty
wall,ice,2thick,wall,2thick,ice,thick,ice,thick,ice,thick,ice,wall,ice,3wall
wall,ice,2thick,wall,2ice,wall,ice,wall,ice,wall,ice,2wall,3ice,wall
wall,ice,2thick,2wall,2ice,thick,ice,thick,ice,thick,ice,3wall,lock,wall
wall,ice,2thick,2wall,2ice,2wall,ice,wall,2ice,wall,goal,2ice,wall
wall,ice,2thick,ice,14wall
wall,4ice,wall,13empty
6wall,13empty;
puffle(15,1)
keys(4,13)
");

        public static readonly Level Level13 = new(@"
map
7empty,4wall,empty,3wall,3empty
2empty,6wall,2ice,3wall,ice,wall,3empty
2empty,wall,2ice,wall,3ice,thick,2ice,2thick,wall,3empty
2empty,wall,7ice,2wall,2ice,wall,3empty
2empty,wall,ice,3wall,7ice,2wall,2empty
2empty,wall,ice,wall,hole,5wall,3thick,ice,wall,2empty
2empty,wall,thick,7ice,thick,2ice,2wall,2empty
2empty,wall,ice,wall,ice,2wall,3ice,3thick,wall,3empty
3wall,ice,wall,ice,wall,6ice,thick,wall,3empty
wall,ice,wall,ice,wall,ice,wall,4ice,3thick,wall,3empty
wall,thick,wall,ice,wall,ice,5wall,thick,ice,5wall
wall,thick,wall,ice,thick,ice,wall,goal,ice,lock,ice,thick,2wall,3ice,wall
wall,thick,2wall,thick,6wall,3ice,thick,2ice,wall
wall,4thick,wall,4empty,8wall
6wall,12empty;
puffle(15,11)
keys(1,9)
blocks(5,9)
");

        public static readonly Level Level14 = new(@"
map
6wall,3empty,4wall,5empty
wall,4ice,wall,3empty,wall,2ice,3wall,3empty
wall,4ice,3wall,empty,wall,4ice,wall,3empty
2wall,5ice,3wall,4ice,2wall,2empty
empty,wall,4ice,thick,2ice,wall,3ice,thick,ice,2wall,empty
2wall,ice,2wall,2ice,wall,ice,2wall,3ice,thick,ice,wall,empty
wall,2ice,2wall,3ice,thick,6ice,3wall
wall,3ice,wall,2ice,thick,9ice,wall
wall,3ice,wall,ice,wall,thick,3wall,ice,wall,4ice,wall
2wall,2ice,wall,2ice,thick,2ice,wall,thick,3ice,3wall
2wall,ice,6wall,ice,wall,3ice,thick,ice,wall,empty
empty,wall,ice,4wall,3ice,wall,2ice,thick,ice,2wall,empty
empty,wall,ice,lock,ice,goal,wall,3ice,wall,3ice,2wall,2empty
empty,wall,hole,4wall,3ice,2wall,2ice,wall,3empty
empty,3wall,2empty,9wall,3empty;
puffle(7,11)
keys(16,7)
blocks(7,8)
");

        public static readonly Level Level15 = new(@"
map
19wall
wall,8ice,wall,2thick,wall,2thick,3ice,wall
wall,ice,wall,7ice,4thick,ice,wall,2ice,wall
wall,3ice,wall,5ice,4thick,ice,wall,lock,2wall
wall,9ice,4thick,ice,wall,ice,goal,wall
wall,5ice,3thick,ice,thick,4ice,4wall
wall,5ice,thick,ice,3thick,7ice,wall
wall,5ice,thick,11ice,wall
2wall,4thick,6ice,thick,5ice,wall
wall,hole,4thick,12ice,wall
wall,2ice,wall,10ice,2wall,2ice,wall
wall,13ice,5wall
wall,ice,wall,ice,wall,ice,wall,ice,wall,9ice,wall
wall,3ice,3wall,11ice,wall
19wall;
puffle(5,12)
keys(1,9)
blocks(5,8)
");

        public static readonly Level Level16 = new(@"
map
19wall
wall,2ice,thick,3ice,2wall,4ice,wall,4ice,wall
wall,ice,wall,11ice,thick,3ice,wall
wall,3ice,2wall,4ice,3wall,2ice,4wall
3wall,ice,3wall,3ice,3wall,5ice,wall
2wall,2ice,3wall,ice,wall,ice,2wall,5ice,2wall
2wall,5ice,thick,ice,thick,7ice,2wall
3wall,3ice,wall,ice,wall,ice,wall,7ice,wall
3wall,2ice,thick,ice,thick,ice,thick,8ice,wall
wall,5ice,wall,ice,wall,ice,9wall
wall,2ice,thick,3ice,thick,ice,thick,2ice,wall,4ice,thick,wall
wall,3ice,wall,2ice,thick,ice,thick,2ice,wall,lock,wall,goal,wall,thick,wall
wall,7ice,wall,4ice,thick,3wall,thick,wall
wall,2ice,wall,4ice,wall,hole,wall,2ice,5thick,wall
19wall;
puffle(17,4)
keys(17,13)
blocks(14,5)
");

        public static readonly Level Level17 = new(@"
map
19wall
wall,5ice,2wall,hole,wall,3ice,wall,ice,wall,2ice,wall
wall,5ice,wall,4ice,wall,ice,wall,thick,3ice,wall
wall,5ice,wall,2ice,3wall,thick,3ice,wall,ice,wall
wall,ice,3wall,ice,wall,2ice,wall,ice,thick,ice,wall,2ice,wall,ice,wall
wall,ice,wall,goal,wall,ice,wall,2ice,2wall,ice,thick,ice,thick,ice,wall,ice,wall
wall,thick,4ice,wall,2ice,wall,ice,thick,3ice,2wall,ice,wall
wall,lock,5wall,2ice,2wall,ice,thick,ice,2wall,2ice,wall
wall,ice,wall,6ice,wall,2ice,thick,2wall,ice,thick,ice,wall
wall,ice,wall,6ice,wall,3ice,2wall,3ice,wall
wall,ice,6wall,ice,8wall,ice,wall
wall,ice,tp,wall,12ice,wall,ice,wall
wall,13ice,3wall,ice,wall
wall,9ice,tp,7ice,wall
19wall;
puffle(15,11)
keys(14,1)
blocks(5,11)
");

        public static readonly Level Level18 = new(@"
map
19wall
2wall,16ice,wall
wall,hole,13ice,tp,2ice,wall
5wall,3ice,thick,9ice,wall
5wall,ice,wall,tp,thick,7ice,wall,ice,wall
3wall,3ice,wall,ice,thick,9ice,wall
7wall,8ice,wall,2ice,wall
3wall,goal,2ice,wall,8ice,wall,2ice,wall
5wall,lock,wall,11ice,wall
5wall,2thick,3ice,wall,5ice,wall,ice,wall
wall,3ice,thick,5ice,wall,7ice,wall
wall,ice,2wall,thick,ice,wall,11ice,wall
wall,ice,2wall,2thick,10ice,wall,ice,wall
wall,3ice,thick,ice,2wall,2ice,2wall,2ice,wall,3ice,wall
19wall;
puffle(3,5)
keys(2,1)
blocks(16,10)
");

        public static readonly Level Level19 = new(@"
map
19wall
wall,goal,wall,10ice,wall,4fakeimpass,wall
wall,5ice,3wall,4ice,faketemp,3fakepass,fakeimpass,wall
4wall,2ice,wall,hole,wall,4ice,3fakeimpass,2fakepass,wall
wall,12ice,wall,3fakepass,fakeimpass,wall
wall,ice,wall,10ice,wall,fakepass,3fakeimpass,wall
wall,ice,3wall,8ice,wall,fakepass,3fakeimpass,wall
wall,lock,wall,7ice,tp,2ice,wall,4fakepass,wall
wall,ice,3wall,5ice,thick,2ice,wall,fakepass,2fakeimpass,fakepass,wall
wall,ice,wall,2thick,4ice,wall,ice,wall,ice,wall,fakeimpass,3fakepass,wall
wall,thick,ice,thick,ice,wall,3ice,wall,3ice,2wall,fakeimpass,fakepass,fakeimpass,wall
wall,ice,wall,ice,2wall,ice,tp,ice,wall,4ice,2wall,faketemp,fakepass,wall
wall,2ice,thick,ice,wall,3ice,wall,3ice,2thick,ice,wall,fakepass,wall
wall,4ice,5wall,2ice,5wall,button,wall
19wall;
puffle(3,7)
keys(15,12)
blocks(12,7)
");

        public static Level[] Levels = new Level[]
        {
            Level1,
            Level2,
            Level3,
            Level4,
            Level5,
            Level6,
            Level7,
            Level8,
            Level9,
            Level10,
            Level11,
            Level12,
            Level13,
            Level14,
            Level15,
            Level16,
            Level17,
            Level18,
            Level19
        };
    }
}

