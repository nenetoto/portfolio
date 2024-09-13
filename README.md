* # Decoration : 아바타 꾸미기
 * ## 나의 아바타를 꾸미기 아이템과 함께 배치하는 콘텐츠 코드 일부입니다.
 * ## Controller 
    * DecorationObjectControllPad
      * 화면에 구성된 아바타 꾸미기 아이템의 크기, 각도, 위치, 좌우 반전을 조절하는 컨트롤 패드입니다.
    *  DecorationObjectControllInput
        * 화면을 구성하는 카메라가 따로 존재하여, UI 인풋과 분별을 위해 DecorationobjectContollPad에 활성화된 오브젝트를 컨트롤하기 위한 인풋을 따로 만들었습니다.
  * ## Decorationobjects
    * BaseDecorationObject
      * 꾸미기 아이템의 리소스가 단순 이미지, 이미지 파츠를 조립한 아바타, 파티클, 스파인으로 이루어져 있어 통합 관리하기 위해 Base 클래스를 생성하여 컨트롤하였습니다.
      * DecorationObjectControllPad를 통해 크기, 각도, 위치, 좌우 반전이 조절되는 오브젝트의 기본 클래스입니다.
    * AvatarDecorationObject
        * 이미지로 구성된 파츠가 조립된 꾸미기 아이템 오브젝트입니다.
     * PerfumeDecorationObject
       * 파티클로 구성된 꾸미기 아이템 오브젝트입니다.
     * BackgroundDecorationObject, NpcDecorationObject, StickerDecorationObject
       *   단순 이미지로 구성된 꾸미기 아이템 오브젝트입니다.
* # Platforms : 플랫폼 매니저와 각 플랫폼에 대한 처리
 * ## 프로젝트 전반의 매니저 처리와 각 플랫폼 상황에 따라 처리된 코드 일부입니다.
 * ## PlatformManager
   * 로컬 노티와 리뷰 기능을 플랫폼에 별로 활성화시키며 필요 기능을 호출하는 역할을 합니다. 매니저는 최대한 간결하고 기능에 대한 함수만을 제공하려고 했으며, 로직은 각 플랫폼에 해당하는 클래스를 작성하여 처리하였습니다.
  * ## PushNotification
  * INotificationWrapper
    * 플랫폼 별로 필요 이상한 기능만 재정의한 인터페이스를 통해 필요한 기능만 노출하도록 처리했습니다.
 * IOSNotificationWrapper
   * iOS 플랫폼 로컬 노티 기능하는 클래스입니다.
 * AndroidNotificationWrapper
   * Android 플랫폼 로컬 노티 기능하는 클래스입니다.
  * ## StoreReview
  * PlatformStoreReview
    * 플랫폼 별로 스토어 리뷰 기능을 공통처리한 클래스입니다.
  * IOSStoreReview
    * iOS 스토어 리뷰 기능하는 클래스입니다.
  * AndroidStoreReview
    * Android 스토어 리뷰 가능하는 클래스입니다.
