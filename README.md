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
