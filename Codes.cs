using System.Collections.Generic;

namespace sojourner;

public static class Codes {
    public static List<List<bool>> codeObjects = [
        [false,true ,false,false,false,true ,false,false,true ,false,false],
        [true ,false,true ,false,true ,true ,false,true ,false,true ,true ],
        [false,false,false,false,false,true ,true ,false,true ,false,false],
        [false,false,true ,false,false,true ,true ,false,true ,false,false],
        [false,false,true ,true ,false,true ,true ,false,true ,true ,false],
        [true ,false,true ,true ,true ,true ,false,false,false,true ,true ],
        [true ,false,false,true ,true ,false,false,false,true ,true ,true ],
        [true ,false,true ,false,true ,false,false,true ,true ,false,true ],
        [true ,false,true ,false,false,false,false,true ,true ,false,false],
        [true ,false,true ,false,false,false,false,true ,false,false,false],
    ];
}