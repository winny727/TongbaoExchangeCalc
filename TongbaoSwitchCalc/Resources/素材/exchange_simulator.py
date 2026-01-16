import random
from typing import List, Dict, Tuple

# ========== 预处理配置 ==========
# 目标通宝配置
TARGET_RARE_TOKEN = "花-界园行"  # 目标稀有通宝
TARGET_PRECIOUS_TOKEN = "花-茧成绢"  # 目标珍贵通宝

# 场景参数配置
INITIAL_PURSE = [
    "衡-志欲遂", "衡-慧避灾", "厉-寒窗志", "花-驰道长",
    "花-百业俱兴", "花-武人之争", "花-修性情", "花-茧成绢", "花-延识镇木",  # 固定区
    "厉-战血流"  # 自由区
]
FREE_SLOTS = 1  # 自由位置数
INITIAL_HP = 101 # 初始血量
IS_TOURIST_TEAM = False  # 是否游客分队
IS_FLOWER_TEAM = True  # 是否花团锦簇分队
HAS_FORTUNE_ARTIFACT = True  # 是否拥有福祸相依

# 模拟参数配置
SIMULATION_COUNT = 1000  # 模拟次数
DEBUG = True  # 是否开启调试输出

# ========== 通宝池定义 ==========
# 初始通宝池
INITIAL_TOKENS = [
    "大炎通宝",
    "厉-西廉贞",
    "厉-南见山",
    "厉-东缺角",
    "厉-北刺面"
]

# 普通通宝池
COMMON_TOKENS = [
    "衡-奇土生金", "衡-水生木护", "衡-金寒水衍", "衡-投木炎延",
    "衡-苦寒", "衡-匪风", "衡-霖雨", "衡-霹雳",
    "衡-旱热", "衡-虹霓", "衡-雾凇", "衡-霜雪",
    "衡-志欲遂", "衡-慧避灾",
    "衡-重铠", "衡-挪移", "衡-迅步",
    "厉-寒窗志",
    "厉-火机",
    "花-火灼土沃", "花-驰道长", "花-武人之争", "花-百业俱兴"
]

# 稀有通宝池
RARE_TOKENS = [
    "衡-庆丰收", "衡-塞上月", "衡-军屯垦",
    "衡-初有文", "衡-勤运体",
    "衡-债难偿",
    "厉-战血流", "厉-遇良弈", "厉-隐市忧", "厉-法与律",
    "厉-两江春", "厉-凡物变", "厉-神农守", "厉-梦奇物",
    "厉-一字落", "厉-待机缘", "厉-画人间", "厉-恣狂情",
    "厉-安硕鼷", "厉-合乎礼", "厉-移山难", "厉-生材百相",
    "花-己任重",
    "花-界园行",
    "厉-黑子伏", "衡-触锁代币", "花-延识镇木"
]

# 珍贵通宝池
PRECIOUS_TOKENS = [
    "衡-鸿蒙开荒",
    "厉-人间长存", "厉-诛邪雷法", "厉-商路难行",
    "花-聚力则强", "花-茧成绢", "花-平沙之盾", "花-火上之灶",
    "花-鸭爵金币", "花-神秘商贾",
    "花-孜孜不倦"
]

# 升级通宝映射
UPGRADE_TOKENS = {
    "厉-移山难": ["衡-移山繁"],
    "衡-勤运体": ["花-修性情", "花-心无患"],
    "衡-初有文": ["花-载道远"]
}

# 升级后的通宝集合
UPGRADED_TOKENS = {
    "衡-移山繁",
    "花-修性情", "花-心无患",
    "花-载道远"
}

# 资源产出通宝
RESOURCE_TOKENS = {
    "衡-庆丰收": {"hp": 2, "ingot": 0, "ticket": 0},
    "衡-债难偿": {"hp": 6, "ingot": 0, "ticket": 0},
    "厉-合乎礼": {"hp": 0, "ingot": 30, "ticket": 0},
    "花-神秘商贾": {"hp": 0, "ingot": 0, "ticket": 4}
}


def get_token_rarity(token: str) -> str:
    """获取通宝稀有度"""
    if token in INITIAL_TOKENS:
        return "initial"
    if token in COMMON_TOKENS:
        return "common"
    if token in RARE_TOKENS:
        return "rare"
    if token in PRECIOUS_TOKENS:
        return "precious"
    # 升级后的通宝按稀有度识别
    if token in UPGRADED_TOKENS:
        return "rare"
    return "common"


def get_exchange_pool(token: str) -> List[str]:
    """获取可交换池"""
    if token in UPGRADE_TOKENS:
        if token == "衡-勤运体":
            return ["花-修性情", "花-心无患"]
        else:
            return UPGRADE_TOKENS[token]
    
    rarity = get_token_rarity(token)
    
    if rarity == "precious":
        return PRECIOUS_TOKENS + ["厉-黑子伏", "衡-触锁代币", "花-延识镇木", "花-武人之争", "花-百业俱兴"]
    elif rarity == "rare":
        return RARE_TOKENS + ["花-平沙之盾", "花-驰道长", "厉-寒窗志", "衡-志欲遂", "衡-慧避灾"]
    else:
        return COMMON_TOKENS


def has_upgrade_conflict(purse: List[str], token: str) -> bool:
    """检查升级通宝冲突"""
    if token not in UPGRADE_TOKENS:
        return False
    
    upgraded_versions = UPGRADE_TOKENS[token]
    
    for upgraded in upgraded_versions:
        if upgraded in purse:
            return True
    
    return False


def perform_exchange(purse: List[str], token_to_exchange: str, has_fortune_artifact: bool) -> Tuple[str, Dict[str, int]]:
    """执行单次交换"""
    resources = {"hp": 0, "ingot": 0, "ticket": 0}
    
    if token_to_exchange in UPGRADE_TOKENS:
        if token_to_exchange == "衡-勤运体":
            new_token = random.choice(["花-修性情", "花-心无患"])
        else:
            new_token = UPGRADE_TOKENS[token_to_exchange][0]
    else:
        exchange_pool = get_exchange_pool(token_to_exchange)
        
        available_tokens = []
        for token in exchange_pool:
            if token in purse:
                continue
            if has_upgrade_conflict(purse, token):
                continue
            available_tokens.append(token)
        
        new_token = random.choice(available_tokens)
    
    if token_to_exchange in RESOURCE_TOKENS:
        resource_gain = RESOURCE_TOKENS[token_to_exchange]
        resources["hp"] += resource_gain["hp"]
        resources["ingot"] += resource_gain["ingot"]
        resources["ticket"] += resource_gain["ticket"]
    
    if has_fortune_artifact and new_token.startswith("厉-"):
        resources["ticket"] += 1
    
    return new_token, resources


def calculate_hp_cost(exchange_count: int, is_flower_team: bool) -> int:
    """计算血量消耗"""
    if is_flower_team:
        return 1
    if exchange_count <= 2:
        return 1
    if exchange_count in [3, 4]:
        return 2
    return 3


def simulate_single_run(initial_purse: List[str], free_slots: int, initial_hp: int,
                       is_tourist_team: bool, is_flower_team: bool, has_fortune_artifact: bool, 
                       target_rare_token: str = None, target_precious_token: str = None,
                       debug: bool = False) -> Dict[str, int]:
    """单次模拟"""
    # 初始钱盒一定是装满的
    purse_capacity = len(initial_purse)
    
    purse = initial_purse.copy()
    
    current_hp = initial_hp
    
    total_hp_consumed = 0
    total_hp_gained = 0
    total_ingot_gained = 0
    total_ticket_gained = 0
    
    exchange_count = 0
    
    # 固定区大小 = 钱盒容量 - 自由位置数
    fixed_zone_size = purse_capacity - free_slots
    
    # 记录获得特定通宝时的血量消耗
    hp_when_got_rare_token = None
    hp_when_got_precious_token = None
    
    if debug:
        print(f"\n【调试模式】")
        print(f"钱盒容量: {purse_capacity}")
        print(f"固定区大小: {fixed_zone_size}")
        print(f"自由区大小: {free_slots}")
        print(f"初始钱盒: {purse}")
        print(f"固定区通宝: {purse[:fixed_zone_size]}")
        print(f"自由区通宝: {purse[fixed_zone_size:]}")
        print(f"初始血量: {current_hp}\n")
    
    # 不断交换自由区的通宝，直到血量耗尽
    while True:
        # 只有自由区（索引 >= fixed_zone_size）的通宝可以交换
        exchangeable_tokens = purse[fixed_zone_size:]
        
        # 如果自由区没有通宝，无法交换
        if not exchangeable_tokens:
            break
        
        hp_cost = calculate_hp_cost(exchange_count + 1, is_flower_team)
        
        if current_hp < hp_cost:
            if debug:
                print(f"\n血量不足！当前血量{current_hp}，需要{hp_cost}，交换结束。")
            break
        
        token_to_exchange = random.choice(exchangeable_tokens)
        
        if debug:
            print(f"--- 第{exchange_count + 1}次交换 ---")
            print(f"  交换前血量: {current_hp}")
            print(f"  本次消耗血量: {hp_cost}")
            print(f"  自由区可交换通宝: {exchangeable_tokens}")
            print(f"  选中交换: {token_to_exchange}")
        
        current_hp -= hp_cost
        total_hp_consumed += hp_cost
        
        new_token, resources = perform_exchange(purse, token_to_exchange, has_fortune_artifact)
        
        if debug:
            print(f"  交换获得: {new_token}")
            print(f"  资源产出: 血量+{resources['hp']}, 源石锭+{resources['ingot']}, 票券+{resources['ticket']}")
        
        purse.remove(token_to_exchange)
        purse.append(new_token)
        
        # 记录第一次获得特定通宝时的血量消耗
        if target_rare_token and new_token == target_rare_token and hp_when_got_rare_token is None:
            hp_when_got_rare_token = total_hp_consumed
        if target_precious_token and new_token == target_precious_token and hp_when_got_precious_token is None:
            hp_when_got_precious_token = total_hp_consumed
        
        total_hp_gained += resources["hp"]
        total_ingot_gained += resources["ingot"]
        total_ticket_gained += resources["ticket"]
        current_hp += resources["hp"]
        
        if debug:
            print(f"  剩余血量: {current_hp}")
            print(f"  当前钱盒: {purse}")
            print(f"  累计: 血量+{total_hp_gained}, 源石锭+{total_ingot_gained}, 票券+{total_ticket_gained}")
            print()
        
        exchange_count += 1
    
    if debug:
        print(f"\n【交换结束】")
        print(f"  总交换次数: {exchange_count}")
        print(f"  消耗血量: {total_hp_consumed}")
        print(f"  获得血量: {total_hp_gained}")
        print(f"  获得源石锭: {total_ingot_gained}")
        print(f"  获得票券: {total_ticket_gained}")
        if target_rare_token:
            print(f"  获得{target_rare_token}时消耗血量: {hp_when_got_rare_token}")
        if target_precious_token:
            print(f"  获得{target_precious_token}时消耗血量: {hp_when_got_precious_token}")
        print(f"  最终钱盒: {purse}")
    
    return {
        "hp_consumed": total_hp_consumed,
        "hp_gained": total_hp_gained,
        "ingot_gained": total_ingot_gained,
        "ticket_gained": total_ticket_gained,
        "hp_when_got_rare_token": hp_when_got_rare_token,
        "hp_when_got_precious_token": hp_when_got_precious_token
    }


def calculate_exchange_expectation(initial_purse: List[str], free_slots: int, initial_hp: int,
                                   is_tourist_team: bool, is_flower_team: bool, 
                                   has_fortune_artifact: bool, simulation_count: int = 1,
                                   target_rare_token: str = None, target_precious_token: str = None,
                                   debug: bool = False, show_progress: bool = True) -> Dict[str, float]:
    """计算交换期望"""
    results = []
    
    # 进度条设置
    progress_step = max(1, simulation_count // 20)  # 每5%输出一次
    last_progress = 0
    
    if show_progress:
        print("模拟进度: ", end='', flush=True)
    
    for i in range(simulation_count):
        result = simulate_single_run(initial_purse, free_slots, initial_hp,
                                     is_tourist_team, is_flower_team, has_fortune_artifact,
                                     target_rare_token, target_precious_token,
                                     debug=(debug and i == 0))  # 只在第一次模拟时输出调试信息
        results.append(result)
        
        # 更新进度条
        if show_progress:
            current_progress = int((i + 1) / simulation_count * 100)
            if current_progress >= last_progress + 5 or i == simulation_count - 1:
                print(f"{current_progress}%", end=' ', flush=True)
                last_progress = current_progress
    
    if show_progress:
        print()  # 换行
    
    avg_hp_consumed = sum(r["hp_consumed"] for r in results) / simulation_count
    avg_hp_gained = sum(r["hp_gained"] for r in results) / simulation_count
    avg_ingot_gained = sum(r["ingot_gained"] for r in results) / simulation_count
    avg_ticket_gained = sum(r["ticket_gained"] for r in results) / simulation_count
    
    # 计算获得特定通宝时的期望血量消耗（只统计获得该通宝的模拟）
    rare_token_hp_list = [r["hp_when_got_rare_token"] for r in results if r["hp_when_got_rare_token"] is not None]
    precious_token_hp_list = [r["hp_when_got_precious_token"] for r in results if r["hp_when_got_precious_token"] is not None]
    
    avg_hp_for_rare_token = sum(rare_token_hp_list) / len(rare_token_hp_list) if rare_token_hp_list else None
    avg_hp_for_precious_token = sum(precious_token_hp_list) / len(precious_token_hp_list) if precious_token_hp_list else None
    
    return {
        "hp_consumed": avg_hp_consumed,
        "hp_gained": avg_hp_gained,
        "net_hp": avg_hp_gained - avg_hp_consumed,
        "ingot_gained": avg_ingot_gained,
        "ticket_gained": avg_ticket_gained,
        "hp_for_rare_token": avg_hp_for_rare_token,
        "hp_for_precious_token": avg_hp_for_precious_token,
        "rare_token_count": len(rare_token_hp_list),
        "precious_token_count": len(precious_token_hp_list)
    }


def run_example_scenarios():
    """运行示例场景"""
    print("=" * 60)
    print("筹谋交换模拟结果")
    print("=" * 60)
    
    # 使用预处理配置进行计算
    result = calculate_exchange_expectation(
        initial_purse=INITIAL_PURSE,
        free_slots=FREE_SLOTS,
        initial_hp=INITIAL_HP,
        is_tourist_team=IS_TOURIST_TEAM,
        is_flower_team=IS_FLOWER_TEAM,
        has_fortune_artifact=HAS_FORTUNE_ARTIFACT,
        simulation_count=SIMULATION_COUNT,
        target_rare_token=TARGET_RARE_TOKEN,
        target_precious_token=TARGET_PRECIOUS_TOKEN,
        debug=DEBUG
    )
    
    # 输出输入配置
    print(f"【输入配置】")
    print(f"初始钱盒: {len(INITIAL_PURSE)}个通宝（固定{len(INITIAL_PURSE)-FREE_SLOTS}+自由{FREE_SLOTS}）")
    print(f"  固定区: {INITIAL_PURSE[:len(INITIAL_PURSE)-FREE_SLOTS]}")
    print(f"  自由区: {INITIAL_PURSE[len(INITIAL_PURSE)-FREE_SLOTS:]}")
    print(f"初始血量: {INITIAL_HP}")
    print(f"花团锦簇分队: {'是' if IS_FLOWER_TEAM else '否'}")
    print(f"游客分队: {'是' if IS_TOURIST_TEAM else '否'}")
    print(f"福祸相依: {'是' if HAS_FORTUNE_ARTIFACT else '否'}")
    print(f"模拟次数: {SIMULATION_COUNT}")
    print(f"目标稀有通宝: {TARGET_RARE_TOKEN}")
    print(f"目标珍贵通宝: {TARGET_PRECIOUS_TOKEN}")
    print()
    
    # 输出结果
    print(f"【模拟结果】")
    print(f"期望消耗血量: {result['hp_consumed']:.2f}")
    print(f"期望获得血量: {result['hp_gained']:.2f}")
    print(f"期望获得源石锭: {result['ingot_gained']:.2f}")
    print(f"期望获得票券: {result['ticket_gained']:.2f}")
    print()
    
    # 输出特定通宝的期望血量消耗
    print(f"【特定通宝获取情况】")
    if result['hp_for_rare_token'] is not None:
        print(f"期望获得{TARGET_RARE_TOKEN}时消耗血量: {result['hp_for_rare_token']:.2f} (成功率: {result['rare_token_count']}/{SIMULATION_COUNT})")
    else:
        print(f"期望获得{TARGET_RARE_TOKEN}时消耗血量: 未获得 (成功率: 0/{SIMULATION_COUNT})")
    
    if result['hp_for_precious_token'] is not None:
        print(f"期望获得{TARGET_PRECIOUS_TOKEN}时消耗血量: {result['hp_for_precious_token']:.2f} (成功率: {result['precious_token_count']}/{SIMULATION_COUNT})")
    else:
        print(f"期望获得{TARGET_PRECIOUS_TOKEN}时消耗血量: 未获得 (成功率: 0/{SIMULATION_COUNT})")


if __name__ == "__main__":
    run_example_scenarios()

