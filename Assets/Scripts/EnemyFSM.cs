using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// 목표: 적을 FSM 다이어그램에 따라 동작시키고 싶다.
// 필요속성1: 적 상태

// 목표2: 플레이어와의 거리를 측정해서 특정 상태로 만들어준다.
// 필요속성2: 플레이어와의 거리, 플레이어 트랜스폼

// 목표3: 적의 상태가 Move일 때, 플레이어와의 거리가 공격 범위 밖이면 적이 플레이어를 따라가고
// 공격 범위 내로 들어오면 공격으로 상태를 전환한다.
// 필요속성3: 이동 속도, 적의 이동을 위한 캐릭터컨트롤러, 공격범위

// 목표4. 플레이어가 공격범위 내에 들어오면 특정 시간에 한번씩 attackPower의 힘으로 공격한다.
// 필요속성4: 현재시간, 특정공격딜레이, attackPower

// 목표5. 플레이어를 따라가다가 초기 위치에서 일정 거리를 벗어나면 Return 상태로 전환한다.
// 필요속성5: 초기위치, 이동가능 범위

// 목표6. 초기 위치로 돌아온다. 특정 거리 이내면, Idle 상태로 전환한다.
// 필요속성6: 특정 거리

// 목표7. 플레이어의 공격을 받으면 hitDamage만큼 에네미의 hp를 감소신다.
// 필요속성7: hp

// 목표8: 2초 후에 내 자신을 제거하겠다.

// 목표9. 현재 에네미의 hp(%)를 hp 슬라이더에 적용한다.
// 필요속성4: hp, maxHp, Slider

// < Alpha upgrade > 
// 목표 10. Idle 상태에서 Move 상태로 Animation 전환을 한다.
// 필요속성10. Animator

// 목표 11. 네비게이션 에이전트의 최소 거리를 입력해 주고, 플레이어를 따라갈 수 있도록 한다. 
// 필요속성11. 네비게이션 에이전트

// 목표 12: 내비게이션 에이전트의 이동을 멈추고 네비게이션 경로를 초기화 해준다.

// 목표 13: Enemy의 초기 속도를 Agent의 속도에 적용하고 싶다.

// 목표 14: 에이전트가 NavMeshLink에 올라고 내려가는지 확인하여 점프 애니메이션을 넣고싶다.
public class EnemyFSM : MonoBehaviour
{
    // 필요속성: 적 상태
    enum EnemyState
    {
        Idle,
        Move,
        Attack,
        Return,
        Damaged,
        Die
    }
    EnemyState enemyState;

    // 필요속성2: 플레이어와의 거리, 플레이어 트랜스폼
    public float findDistance = 5f;
    Transform player;

    // 필요속성3: 이동 속도, 적의 이동을 위한 캐릭터컨트롤러, 공격범위
    public float moveSpeed;
    CharacterController characterController; // cc
    public float attackDistance = 2f;

    // 필요속성4: 현재시간, 특정공격딜레이, attackPower
    float currentTime = 0;
    public float attackDelay = 2f;
    public int attackPower = 3;

    // 필요속성5: 초기위치, 이동가능 범위
    Vector3 originPos;
    public float moveDistance = 20f;

    // 필요속성6: 특정 거리
    float returnDistance = 0.3f;

    // 필요속성7: hp
    public int hp = 3;

    // 필요속성4: hp, maxHp, Slider
    int maxHP = 3;
    public Slider hpSlider;

    // 필요속성10. Animator
    Animator animator;

    // 필요속성11. 네비게이션 에이전트
    NavMeshAgent navMeshAgent;

    // Start is called before the first frame update
    void Start()
    {
        enemyState = EnemyState.Idle;

        player = GameObject.Find("Player").transform;

        characterController = GetComponent<CharacterController>();

        originPos = transform.position;

        maxHP = hp;

        animator = GetComponentInChildren<Animator>();

        navMeshAgent = GetComponent<NavMeshAgent>();

        // 목표 13: Enemy의 초기 속도를 Agent의 속도에 적용하고 싶다.
        navMeshAgent.speed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // 목적7: GameManager가 Ready 상태일 때는 플레이어, 적이 움직일 수 없도록 한다.
        if (GameManager.Instance.state != GameManager.GameState.Start)
            return;

        // 목표: 적을 FSM 다이어그램에 따라 동작시키고 싶다.
        switch (enemyState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Move:
                Move();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Return:
                Return();
                break;
            case EnemyState.Damaged:
                //Damaged();
                break;
            case EnemyState.Die:
                //Die();
                break;
        }

        // 목적9. 현재 에네미의 hp(%)를 hp 슬라이더에 적용한다.
        hpSlider.value = (float)hp / maxHP;
    }

    private void Die()
    {
        StopAllCoroutines();

        StartCoroutine(DieProcess());
    }

    // 목적8: 2초 후에 내 자신을 제거하겠다.
    IEnumerator DieProcess()
    {
        animator.SetTrigger("Die");

        yield return new WaitForSeconds(2);

        print("사망");
        Destroy(gameObject);
    }

    // 목표7. 플레이어의 공격을 받으면 damage만큼 에네미의 hp를 감소시킨다.
    // 목표8. 에네미의 체력이 0보다 크면 피격 상태로 전환
    // 목표9. 그렇지 않으면 죽음 상태로 전환
    public void DamageAction(int damage)
    {
        // 만약, 이미 에네미가 피격됐거나, 사망 상태라면 데미지를 주지 않는다.
        if (enemyState == EnemyState.Damaged || enemyState == EnemyState.Die)
            return;

        // 플레이어의 공격력 만큼 hp를 감소
        hp -= damage;

        // 목표 12: 내비게이션 에이전트의 이동을 멈추고 네비게이션 경로를 초기화 해준다.
        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();

        // 목표8. 에네미의 체력이 0보다 크면 피격 상태로 전환
        if (hp > 0)
        {
            enemyState = EnemyState.Damaged;
            print("상태 전환: Any state -> Damaged");
            Damaged();
        }
        // 목표9. 그렇지 않으면 죽음 상태로 전환
        else
        {
            enemyState = EnemyState.Die;
            print("상태 전환: Any state -> Die");
            Die();
        }
    }

    private void Damaged()
    {
        // 피격 모션 0.5
        animator.SetTrigger("Damaged");

        // 피격 상태 처리를 위한 코루틴 실행
        StartCoroutine(DamageProcess());
    }

    // 데미지 처리용
    IEnumerator DamageProcess()
    {
        // 피격 모션 시간만큼 기다린다.
        yield return new WaitForSeconds(0.5f);

        // 현재 상태를 이동 상태로 전환한다.
        enemyState = EnemyState.Move;
        print("상태 전환: Damaged -> Move");

        animator.SetTrigger("Damaged2Move");
    }

    // 목표6. 초기 위치로 돌아온다. 특정 거리 이내면, Idle 상태로 전환한다.
    private void Return()
    {
        float distanceToOriginPos = (originPos - transform.position).magnitude;
        // 초기 위치로 돌아온다.
        if ( distanceToOriginPos > returnDistance)
        {
            //Vector3 dir = (originPos - transform.position).normalized;
            //characterController.Move(dir * moveSpeed * Time.deltaTime);

            // 목적지를 초기 위치로 설정
            navMeshAgent.destination = originPos;

            // 네비게이션으로 접근하는 최소 거리 초기화
            navMeshAgent.stoppingDistance = 0;
        }
        // 특정 거리(0.1) 이내면, Idle 상태로 전환한다.
        else
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();

            enemyState = EnemyState.Idle;
            print("상태 전환: Return -> Idle");

            animator.SetTrigger("Return2Idle");
        }
    }

    private void Attack()
    {
        // 목표4. 플레이어가 공격범위 내에 들어오면 특정 시간에 한번씩 공격한다.
        float distanceToPlayer = (player.position - transform.position).magnitude;
        if (distanceToPlayer <= attackDistance)
        {
            currentTime += Time.deltaTime;
            // 특정 시간에 한번씩 공격한다.
            if(currentTime > attackDelay)
            {
                if (player.GetComponent<PlayerMove>().hp < 0) return;

                //player.GetComponent<PlayerMove>().DamageAction(attackPower);
                print("공격!");
                currentTime = 0;

                animator.SetTrigger("AttackDelay2Attack");
            }
        }
        else 
        {
            // 그렇지 않으면 Move로 상태를 전환한다.
            enemyState = EnemyState.Move;
            print("상태 전환: Attack -> Move");
            currentTime = 0;

            animator.SetTrigger("Attack2Move");
        }
    }

    public void AttackAction()
    {
        player.GetComponent<PlayerMove>().DamageAction(attackPower);
    }

    // 목표3: 적의 상태가 Move일 때, 플레이어와의 거리가 공격 범위 밖이면 적이 플레이어를 따라간다.
    private void Move()
    {
        // 플레이어와의 거리가 공격 범위 밖이면 적이 플레이어를 따라간다.
        float distanceToPlayer = (player.position - transform.position).magnitude;

        // 목표5. 플레이어를 따라가다가 초기 위치에서 일정 거리를 벗어나면 초기 위치로 돌아온다.
        float distanceToOriginPos = (originPos -  transform.position).magnitude;
        if(distanceToOriginPos > moveDistance)
        {
            enemyState = EnemyState.Return;
            print("상태 전환: Move -> Return");

            transform.forward = (originPos - transform.position).normalized;

            animator.SetTrigger("Move2Return");
        }
        else if(distanceToPlayer > attackDistance)
        {
            // 목표 14: 에이전트가 NavMeshLink에 올라고 내려가는지 확인하여 점프 애니메이션을 넣고싶다.
            if(navMeshAgent.isOnOffMeshLink)
            {
                object navMeshOwner = navMeshAgent.navMeshOwner;
                GameObject navMeshGO = (navMeshOwner as Component).gameObject;

                print(navMeshGO.name + "로 이동하고 있습니다.");
            }

            //Vector3 dir = (player.position - transform.position).normalized;

            //// 플레이어를 따라간다.
            //characterController.Move(dir * moveSpeed * Time.deltaTime);

            //transform.forward = dir;

            // 이동을 멈추고 경로를 초기화한다.
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();

            // 목표 11. 네비게이션 에이전트의 최소 거리를 입력해 주고, 플레이어를 따라갈 수 있도록 한다. 
            navMeshAgent.stoppingDistance = attackDistance;
            navMeshAgent.SetDestination(player.position);

        }
        else
        {
            // 공격 범위 내로 들어오면 공격으로 상태를 전환한다.
            enemyState = EnemyState.Attack;
            print("상태 전환: Move -> Attack");
            currentTime = attackDelay;

            animator.SetTrigger("Move2AttackDelay");
        }
    }

    // 목표2: 플레이어와의 거리를 측정해서 Move 상태로 만들어준다.
    private void Idle()
    {
        float distanceToPlayer = (player.position - transform.position).magnitude;
        // float tempDist = Vector3.Distance(transform.position, player.position);

        // 현재 플레이어와의 거리가 특정 범위 내면 상태를 Move로 바꿔준다.
        if(distanceToPlayer < findDistance)
        {
            enemyState = EnemyState.Move;
            print("상태전환: Idle -> Move");

            // 목표 10. Idle 상태에서 Move 상태로 Animation 전환을 한다.
            animator.SetTrigger("Idle2Move");
        }
    }
}
