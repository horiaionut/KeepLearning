using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementsFinderController : MiniGame
{
    public static int NumberOfCorectItems = 10;
    public static int NumberOfWrongItems = 20;

    public GameObject ItemPrefab;
    public Transform GameSpace;
    public Text ScoreText;
    public GameObject GameInfo;

    public int CorectItemsCount;
    public int WrongItemsCount;

    public override void StartGame(Category domain, GameObject canvas)
    {
        Domain = domain;
        GameInfo.transform.SetParent(transform, false);

        canvas.SetActive(false);

        Domain.GetSubcategoriesElements();
        Domain.DestroyUnusedSubcategories();

        Initialize();
    }

    void Awake()
    {
        GameSpace = transform.FindChild("GameSpace");
        ItemPrefab = Resources.Load<GameObject>("Prefabs/ElementsFinder/Item");
        GameInfo = transform.FindChild("GameInfo").gameObject;
        ScoreText = GameInfo.transform.FindChild("Score").GetComponent<Text>();
    }

    void Initialize()
    {
        Category CorectCategory = CategoryRandomer.ChooseSubcategory(Domain, CanBeCorectCategory);
        GameInfo.transform.FindChild("CategoryName").GetComponent<Text>().text = CorectCategory.Name;

        AddCorectItems(CorectCategory);
        AddIncorectItems(CorectCategory);

        RefreshScore();
        CorectCategory = null;
        Domain = null;
    }

    void AddCorectItems(Category CorectCategory)
    {
        if (CorectCategory == null)
            throw new System.Exception("No category match!");

        AddImages(CategoryRandomer.ChooseItems(CorectCategory.Images, Mathf.Min(NumberOfCorectItems, CorectCategory.Images.Count)), true);
        if(CorectCategory.Images.Count < NumberOfCorectItems)
            AddWords(CategoryRandomer.ChooseItems(CorectCategory.Words, 
                Mathf.Min(NumberOfCorectItems - CorectCategory.Images.Count, CorectCategory.Words.Count)), true);

        GameItem.GameItemChoosed += ItemChoosed;
    }

    void AddIncorectItems(Category CorectCategory)
    {
        int imagesNumber = Domain.Images.Count - CorectCategory.Images.Count;
        AddImages(CategoryRandomer.GetRandomImages(Domain, Mathf.Min(NumberOfWrongItems, imagesNumber),
            delegate (Category category)
            {
                return category != CorectCategory;
            }), false);

        if(imagesNumber < NumberOfWrongItems)
            AddWords(CategoryRandomer.GetRandomWords(Domain, 
                Mathf.Min(NumberOfWrongItems - imagesNumber, Domain.Words.Count - CorectCategory.Words.Count),
                delegate (Category category)
                {
                    return category != CorectCategory;
                }), false);
    }

    public static bool CanBeCorectCategory(Category category)
    {
        if (category.Words.Count + category.Images.Count >= NumberOfCorectItems)
            return true;
        else
            return false;
    }

    public static bool CanBeCorectCategory(CategoryInfo category)
    {
        if (category.WordsCount + category.ImagesCount >= NumberOfCorectItems)
            return true;
        else
            return false;
    }

    void AddWords(IList<string> words, bool isCorectWord)
    {
        foreach(string word in words)
        {
            ElementsFinderItem item = Instantiate(ItemPrefab).GetComponent<ElementsFinderItem>();
            item.IsCorectItem = isCorectWord;
            item.transform.SetParent(GameSpace, false);
            item.gameObject.GetComponentInChildren<Text>().text = word;
            item.SetRandom();
        }
    }

    void AddImages(IList<string> images, bool isCorectImage)
    {
        foreach (string image in images)
        {
            ElementsFinderItem item = Instantiate(ItemPrefab).GetComponent<ElementsFinderItem>();
            item.IsCorectItem = isCorectImage;
            item.transform.SetParent(GameSpace, false);
            item.GetComponent<Image>().sprite = Resources.Load<Sprite>(image);
            item.SetRandom();
        }
    }

    void ItemChoosed(GameItem item)
    {
        if(item.IsCorectItem)
        {
            CorectItemsCount++;
            if (CorectItemsCount == NumberOfCorectItems)
                GameFinished();
        }
        else
        {
            WrongItemsCount++;
            if (WrongItemsCount > 3)
                GameFinished();
        }
        RefreshScore();
        Destroy(item);
    }

    void RefreshScore()
    {
        ScoreText.text = "Corect: " + CorectItemsCount + " Wrong: " + WrongItemsCount;
    }

    internal override int GetPoints()
    {
        if (CorectItemsCount < NumberOfCorectItems)
            return 0;
        else
            return CorectItemsCount;
    }

    internal override void Refresh()
    {
        GameItem.Refresh();
    }
}