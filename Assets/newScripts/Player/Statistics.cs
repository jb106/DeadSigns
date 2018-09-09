using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType { ConstantIncrease, ConstantDecrease, Static }

[System.Serializable]
public class PlayerStat
{
    public string _name;
    public StatType statType;
    public float _value;
    public float _oldValue;
    public float _maxValue;
    public float _changeValue;
    public float _changeDelay;
    public float _restoreNoEmpty;

    public bool _canRegen = true;
    public bool _empty = false;
    public bool _hasChanged = true;

    public bool _degradation = false;

    public float _playerConsommationValue;

    public void changeValue(float value)
    {
        //Si en ajoutant on dépasse le maximum alors il faut rendre la valeur égale au maximum
        if (_value + value > _maxValue)
            _value = _maxValue;

        //Sinon si on soustrait et que la valeur passe en dessous de zéro il faut éviter qu'elle ne rentre en négatif
        else if (_value + value <= 0)
        {
            _value = 0;
            _empty = true;
        }
        //Sinon la valeur est simplement incrémenté ou décrémenté suivant le signe de value
        else
        {
            _value += value;
            if (_value > _maxValue * _restoreNoEmpty)
                _empty = false;
        }

        if(_value!=_oldValue)
            _hasChanged = true;

        _oldValue = _value;

    }

}

public class Statistics : MonoBehaviour {

    //Référence pour une instance statique, afin d'y accéder de n'importe où
    public static Statistics Instance = null;

    private void Awake()
    {
        Instance = this;
    }


    public List<PlayerStat> _playerStats = new List<PlayerStat>();


    public PlayerStat getPlayerStat(string name)
    {
        foreach (PlayerStat stat in _playerStats)
            if (name == stat._name)
                return stat;



        return null;
    }

    void Start()
    {
        for (int x = 0; x < _playerStats.Count; x++)
        {
            StartCoroutine(statUpdate(x));
        }
    }

    IEnumerator statUpdate(int statIndex)
    {
        PlayerStat stat = _playerStats[statIndex];
        while (true)
        {
            if(stat.statType==StatType.ConstantIncrease)
            {
                if(stat._canRegen)
                {
                    stat.changeValue(stat._changeValue);
                    yield return new WaitForSeconds(stat._changeDelay);
                }
            }
            else if (stat.statType == StatType.ConstantDecrease)
            {
                stat.changeValue(stat._changeValue);
                yield return new WaitForSeconds(stat._changeDelay);
            }

            yield return null;
        }
    }

}
