using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layout : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Dictionary<char, string> symbol_dict =  new Dictionary<char, string>(){
            {'%', "wall"},
            {'.', "food"},
            {'P', "pacman"},
            {'G', "ghost"}
        };
        // layout w x h = 36 x 14
        string [] layout = {
            "%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%",
            "%               3%%2               %",
            "% %%%%%%%%%%%%%% %%%%%%%%%%%%%%%%% %",
            "% %.........   %   ..............% %",
            "% %........  %.%%%%%%%%.%%% %%.%%% %",
            "% %%%%.%%%%% %...  .%.....% %....% %",
            "%  ....%5%%%.%%%.  .......% %....  %",
            "%  ....% %.......  .%%%.%%%6%....  %",
            "% %....% %.....%.  ...% %%%%%.%%%% %",
            "% %%% %% %%%.%%%%%%%%.%  ........% %",
            "% %..............   %   .........% %",
            "% %%%%%%%%%%%%%%%%% %%%%%%%%%%%%%% %",
            "%               1%%4               %",
            "%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%"
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
