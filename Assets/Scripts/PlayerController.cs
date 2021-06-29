using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    private Rigidbody2D m_rg;

    public float MoveSpeed;
    public float JumpSpeed;


    void Start () {

        m_rg = gameObject.GetComponent<Rigidbody2D>();

    }
    
    // Update is called once per frame
    void Update () {
        //------------------Input.GetAxisRaw没有小数值，只有整数，不会产生缓动------------------
        //角色水平移动
        //按住D键，判断如果大于0，则向右开始移动
        if(Input.GetButtonDown("Jump"))
        {
            m_rg.velocity = new Vector2(m_rg.velocity.x, JumpSpeed);
        }
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            m_rg.velocity = new Vector2(MoveSpeed, m_rg.velocity.y);

            //设置自身缩放的值
            //transform.localScale = new Vector2(1f,1f);
        }
        //角色水平移动
        //按住A键，判断如果小于0，则向左开始移动
        else if (Input.GetAxisRaw("Horizontal") < 0)
        {
            m_rg.velocity = new Vector2(-MoveSpeed, m_rg.velocity.y);

            //如果new Vector2(-1f, 1f)  x值为负数，则图片进行反转显示
            //transform.localScale = new Vector2(-1f, 1f);
        }
        else
        //角色水平移动
        //松开按键，判断如果等于0，则停止移动
        {
            m_rg.velocity = new Vector2(0, m_rg.velocity.y);
        }

    }
}