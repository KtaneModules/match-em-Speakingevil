using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MatchemScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMSelectable[] cards;
    public KMSelectable progressor;
    public Renderer[] cardfronts;
    public Renderer[] progrend;
    public Material[] cardmats;
    public Material[] progmats;
    public TextMesh[] cardlabels;
    public TextMesh counter;
    public TextMesh proglabel;

    private int[][] positions = new int[9][] { new int[25] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, new int[25], new int[25], new int[25], new int[25], new int[25], new int[25], new int[25], new int[25]};
    private int[] pairs = new int[25] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
    private string[][][] cardconfig = new string[2][][] { new string[5][] { new string[5], new string[5], new string[5], new string[5], new string[5] }, new string[5][] { new string[5], new string[5], new string[5], new string[5], new string[5] } };
    private int[] colours = new int[13];
    private List<int>[] moves = new List<int>[8] { new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }};
    private bool[] matched = new bool[25];
    private bool wait;
    private int stage = 9;
    private int[] select = new int[2] { -1, -1};

    private static int moduleIDCounter = 1;
    private int moduleID;
    private bool moduleSolved;

    void Awake()
    {
        moduleID = moduleIDCounter++;
        pairs.Shuffle();
        for(int i = 0; i < 13; i++)
        {
            colours[i] = Random.Range(0, 40);
            while(colours.Where((x, k) => k < i).Contains(colours[i]))
                colours[i] = Random.Range(0, 40);
            cardfronts[pairs[2 * i]].material = cardmats[colours[i]];
            cardlabels[pairs[2 * i]].text = "BCFLOPRVWY"[colours[i] % 10].ToString();
            cardconfig[0][pairs[2 * i] / 5][pairs[2 * i] % 5] = "BCFLOPRVWY"[colours[i] % 10].ToString() + new string[] { "\x2663", "\x2666", "\x2665", "\x2660" }[colours[i] / 10];
            if (i < 12)
            {
                cardfronts[pairs[(2 * i) + 1]].material = cardmats[colours[i]];
                cardlabels[pairs[(2 * i) + 1]].text = "BCFLOPRVWY"[colours[i] % 10].ToString();
                cardconfig[0][pairs[(2 * i) + 1] / 5][pairs[(2 * i) + 1] % 5] = "BCFLOPRVWY"[colours[i] % 10].ToString() + new string[] { "\x2663", "\x2666", "\x2665", "\x2660" }[colours[i] / 10];
            }
        }
        Debug.LogFormat("[Match 'em #{0}] The initial configuration of cards is: \n[Match 'em #{0}] {1}", moduleID, string.Join("\n[Match 'em #" + moduleID.ToString() + "] ", cardconfig[0].Select(j => string.Join(" ", j)).ToArray()));
        for (int i = 0; i < 8; i++)
        {
            moves[i].Add(Random.Range(0, 7));
            switch (moves[i][0])
            {
                case 0:
                    int[] r = new int[2] { Random.Range(0, 5), Random.Range(0, 5) };
                    moves[i].Add(r[0]);
                    while (r[0] == r[1])
                        r[1] = Random.Range(0, 5);
                    moves[i].Add(r[1]);
                    for (int j = 0; j < 25; j++)
                        if (j / 5 == moves[i][1])
                            positions[i + 1][j] = positions[i][moves[i][2] * 5 + (j % 5)];
                        else if (j / 5 == moves[i][2])
                            positions[i + 1][j] = positions[i][moves[i][1] * 5 + (j % 5)];
                        else
                            positions[i + 1][j] = positions[i][j];
                    Debug.LogFormat("[Match 'em #{0}] Move {1}: Swap rows {2} and {3}", moduleID, i + 1, moves[i][1] + 1, moves[i][2] + 1);
                    break;
                case 1:
                    r = new int[2] { Random.Range(0, 5), Random.Range(0, 5) };
                    moves[i].Add(r[0]);
                    while (r[0] == r[1])
                        r[1] = Random.Range(0, 5);
                    moves[i].Add(r[1]);
                    for (int j = 0; j < 25; j++)
                        if (j % 5 == moves[i][1])
                            positions[i + 1][j] = positions[i][moves[i][2] + (j / 5) * 5];
                        else if (j % 5 == moves[i][2])
                            positions[i + 1][j] = positions[i][moves[i][1] + (j / 5) * 5];
                        else
                            positions[i + 1][j] = positions[i][j];
                    Debug.LogFormat("[Match 'em #{0}] Move {1}: Swap columns {2} and {3}", moduleID, i + 1, moves[i][1] + 1, moves[i][2] + 1);
                    break;
                case 2:
                    moves[i].Add(Random.Range(0, 5));
                    bool t = Random.Range(0, 2) == 1;
                    for (int j = 0; j < 25; j++)
                        if (j / 5 == moves[i][1])
                        {
                            if (t)
                                positions[i + 1][j] = positions[i][moves[i][1] * 5 + ((j + 1) % 5)];
                            else
                                positions[i + 1][j] = positions[i][moves[i][1] * 5 + ((j + 4) % 5)];
                        }
                        else
                            positions[i + 1][j] = positions[i][j];
                    Debug.LogFormat("[Match 'em #{0}] Move {1}: Shift row {2} one space {3}", moduleID, i + 1, moves[i][1] + 1, t ? "left" : "right");
                    break;
                case 3:
                    moves[i].Add(Random.Range(0, 5));
                    t = Random.Range(0, 2) == 1;
                    for (int j = 0; j < 25; j++)
                        if (j % 5 == moves[i][1])
                        {
                            if (t)
                                positions[i + 1][j] = positions[i][(j + 5) % 25];
                            else
                                positions[i + 1][j] = positions[i][(j + 20) % 25];
                        }
                        else
                            positions[i + 1][j] = positions[i][j];
                    Debug.LogFormat("[Match 'em #{0}] Move {1}: Shift column {2} one space {3}", moduleID, i + 1, moves[i][1] + 1, t ? "up" : "down");
                    break;
                case 4:
                    moves[i].Add(Random.Range(0, 6));
                    for (int j = 0; j < 25; j++)
                    {
                        positions[i + 1][j] = positions[i][j];
                    }
                    switch (moves[i][1])
                    {
                        case 0:
                            positions[i + 1][0] = positions[i][4];
                            positions[i + 1][4] = positions[i][24];
                            positions[i + 1][24] = positions[i][20];
                            positions[i + 1][20] = positions[i][0];
                            break;
                        case 1:
                            positions[i + 1][1] = positions[i][9];
                            positions[i + 1][9] = positions[i][23];
                            positions[i + 1][23] = positions[i][15];
                            positions[i + 1][15] = positions[i][1];
                            break;
                        case 2:
                            positions[i + 1][2] = positions[i][14];
                            positions[i + 1][14] = positions[i][22];
                            positions[i + 1][22] = positions[i][10];
                            positions[i + 1][10] = positions[i][2];
                            break;
                        case 3:
                            positions[i + 1][3] = positions[i][19];
                            positions[i + 1][19] = positions[i][21];
                            positions[i + 1][21] = positions[i][5];
                            positions[i + 1][5] = positions[i][3];
                            break;
                        case 4:
                            positions[i + 1][6] = positions[i][8];
                            positions[i + 1][8] = positions[i][18];
                            positions[i + 1][18] = positions[i][16];
                            positions[i + 1][16] = positions[i][6];
                            break;
                        case 5:
                            positions[i + 1][7] = positions[i][13];
                            positions[i + 1][13] = positions[i][17];
                            positions[i + 1][17] = positions[i][11];
                            positions[i + 1][11] = positions[i][7];
                            break;
                    }
                    Debug.LogFormat("[Match 'em #{0}] Move {1}: Cycle card {2} anticlockwise", moduleID, i + 1, new string[] { "1", "2", "3", "6", "7", "8"}[moves[i][1]]);
                    break;
                case 5:
                    moves[i].Add(Random.Range(0, 6));
                    for (int j = 0; j < 25; j++)
                    {
                        positions[i + 1][j] = positions[i][j];
                    }
                    switch (moves[i][1])
                    {
                        case 0:
                            positions[i + 1][0] = positions[i][20];
                            positions[i + 1][4] = positions[i][0];
                            positions[i + 1][24] = positions[i][4];
                            positions[i + 1][20] = positions[i][24];
                            break;
                        case 1:
                            positions[i + 1][1] = positions[i][15];
                            positions[i + 1][9] = positions[i][1];
                            positions[i + 1][23] = positions[i][9];
                            positions[i + 1][15] = positions[i][23];
                            break;
                        case 2:
                            positions[i + 1][2] = positions[i][10];
                            positions[i + 1][14] = positions[i][2];
                            positions[i + 1][22] = positions[i][14];
                            positions[i + 1][10] = positions[i][22];
                            break;
                        case 3:
                            positions[i + 1][3] = positions[i][5];
                            positions[i + 1][19] = positions[i][3];
                            positions[i + 1][21] = positions[i][19];
                            positions[i + 1][5] = positions[i][21];
                            break;
                        case 4:
                            positions[i + 1][6] = positions[i][16];
                            positions[i + 1][8] = positions[i][6];
                            positions[i + 1][18] = positions[i][8];
                            positions[i + 1][16] = positions[i][18];
                            break;
                        case 5:
                            positions[i + 1][7] = positions[i][11];
                            positions[i + 1][13] = positions[i][7];
                            positions[i + 1][17] = positions[i][13];
                            positions[i + 1][11] = positions[i][17];
                            break;
                    }
                    Debug.LogFormat("[Match 'em #{0}] Move {1}: Cycle card {2} clockwise", moduleID, i + 1, new string[] { "1", "2", "3", "6", "7", "8" }[moves[i][1]]);
                    break;
                default:
                    r = new int[2] { Random.Range(0, 25), Random.Range(0, 25) };
                    moves[i].Add(r[0]);
                    while (r[0] == r[1])
                        r[1] = Random.Range(0, 25);
                    moves[i].Add(r[1]);
                    for (int j = 0; j < 25; j++)
                        if(j == r[0])
                            positions[i + 1][j] = positions[i][r[1]];
                        else if(j == r[1])
                            positions[i + 1][j] = positions[i][r[0]];
                        else
                            positions[i + 1][j] = positions[i][j];
                    Debug.LogFormat("[Match 'em #{0}] Move {1}: Swap cards {2} and {3}", moduleID, i + 1, moves[i][1] + 1, moves[i][2] + 1);
                    break;
            }
        }
        for (int i = 0; i < 25; i++)
            cardconfig[1][Array.IndexOf(positions[8],pairs[i]) / 5][Array.IndexOf(positions[8],pairs[i]) % 5] = "BCFLOPRVWY"[colours[i / 2] % 10].ToString() + new string[] { "\x2663", "\x2666", "\x2665", "\x2660" }[colours[i / 2] / 10];       
        Debug.LogFormat("[Match 'em #{0}] The final configuration of cards is: \n[Match 'em #{0}] {1}", moduleID, string.Join("\n[Match 'em #" + moduleID.ToString() + "] ", cardconfig[1].Select(j => string.Join(" ", j)).ToArray()));
        module.OnActivate += Activate;
    }

    private void Activate()
    {
        foreach (Renderer prog in progrend)
            prog.material = progmats[0];
        proglabel.text = "START";
        progressor.OnInteract += delegate () 
        {
            if (!wait)
            {
                if(stage == 9)
                {
                    stage--;
                    progressor.AddInteractionPunch(0.2f);
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                    Audio.PlaySoundAtTransform("Cardflip", transform);
                    StartCoroutine(Wait(0.5f, true));
                    for (int i = 0; i < 25; i++)
                        StartCoroutine(Flip(i));
                }
                else if (stage > 0)
                {
                    stage--;
                    progressor.AddInteractionPunch(0.2f);
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                    Audio.PlaySoundAtTransform("Cardshift", transform);
                    StartCoroutine(Wait(1, true));
                    foreach (int c in positions[7 - stage].Where((x, i) => x != positions[8 - stage][i]))
                        StartCoroutine(Move(c, Array.IndexOf(positions[8 - stage], c)));
                    if(stage < 1)
                    {
                        foreach(KMSelectable card in cards)
                        {
                            int c = Array.IndexOf(cards, card);
                            card.OnInteract += delegate { if(!wait && !moduleSolved && !matched[c] && !select.Contains(c)) CardSelect(c); return false; };
                        }
                    }
                }
            }
        return false; };
    }

    private IEnumerator Wait(float t, bool p)
    {
        wait = true;
        if (p)
        {
            foreach (Renderer prog in progrend)
                prog.material = progmats[1];
            proglabel.text = "WAIT";
            proglabel.color = new Color32(128, 128, 128, 255);
        }
        yield return new WaitForSeconds(t);
        wait = false;
        if (p)
        {
            foreach (Renderer prog in progrend)
                prog.material = stage > 0 ? progmats[0] : progmats[2];
            proglabel.text = stage > 0 ? "NEXT" : "";
            proglabel.color = new Color32(0, 203, 0, 255);
            counter.text = stage > 0 ? stage.ToString() : "GO";
        }
    }

    private IEnumerator Flip(int card)
    {
        cards[card].transform.localPosition += new Vector3(0, 0.006f, 0);
        for (int i = 0; i < 30; i++)
        {
            cards[card].transform.localEulerAngles += new Vector3(0, 0, 6);
            yield return null;
        }
        cards[card].transform.localPosition -= new Vector3(0, 0.006f, 0);
    }

    private IEnumerator Move(int card, int end)
    {
        float[][] coords = new float[2][] { new float[2] { cards[card].transform.localPosition.x, cards[card].transform.localPosition.z }, new float[2] { new float[] { -0.0418f, -0.0213f, -0.0008f, 0.0197f, 0.0402f }[end % 5], new float[]{0.0555f, 0.0277f, 0, -0.0272f, -0.0544f }[end / 5] } };
        for(int i = 0; i < 45; i++)
        {
            yield return null;
            cards[card].transform.localPosition = new Vector3(Mathf.Lerp(coords[0][0], coords[1][0], (float)i / 44), 0.001f, Mathf.Lerp(coords[0][1], coords[1][1], (float)i / 44));
        }
        cards[card].transform.localPosition -= new Vector3(0, 0.002f, 0);
    }

    private void CardSelect(int card)
    {
        StartCoroutine(Flip(card));
        Audio.PlaySoundAtTransform("Cardflip", transform);
        if (select.All(s => s == -1))
        { 
            StartCoroutine(Wait(0.5f, false));
            select[0] = card;
            if (Array.IndexOf(pairs, card) == 24)
                Debug.LogFormat("[Match 'em #{0}] Joker selected. Strike imminent.", moduleID);
            else
                Debug.LogFormat("[Match 'em #{0}] Card {1} selected. Expecting card {2}.", moduleID, Array.IndexOf(positions[8], card) + 1, Array.IndexOf(pairs, card) % 2 == 1 ? Array.IndexOf(positions[8], pairs[Array.IndexOf(pairs, card) - 1]) + 1: Array.IndexOf(positions[8], pairs[Array.IndexOf(pairs, card) + 1]) + 1);
        }
        else
        {
            StartCoroutine(Wait(1.5f, false));
            select[1] = card;
            Debug.LogFormat("[Match 'em #{0}] Card {1} selected. This is {2}a matching pair.", moduleID, Array.IndexOf(positions[8], card) + 1, Array.IndexOf(pairs, card) / 2 == Array.IndexOf(pairs, select[0]) / 2 ? "" : "not ");
            StartCoroutine(Check());
        }
    }

    private IEnumerator Check()
    {
        yield return new WaitForSeconds(0.5f);
        if(Array.IndexOf(pairs, select[0]) / 2 == Array.IndexOf(pairs, select[1]) / 2)
        {
            matched[select[0]] = true;
            matched[select[1]] = true;
            wait = false;
            if(matched.Where(t => t).Count() == 24)
            {
                moduleSolved = true;
                module.HandlePass();
                counter.text = "GG";
                counter.color = new Color32(0, 255, 0, 255);
            }
        }
        else
        {
            module.HandleStrike();
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(Flip(select[0]));
            StartCoroutine(Flip(select[1]));
        }
        select = new int[2] { -1, -1 };
    }
}
