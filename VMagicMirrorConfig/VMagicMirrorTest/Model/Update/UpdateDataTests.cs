using NUnit.Framework;

namespace Baku.VMagicMirrorConfig.Test
{
    public class UpdateDataTests
    {
        [TestCase("v1.2.3")]
        [TestCase("1.2.3", Description = "�`����v�͖����Ă�����")]
        [Test]
        public void Test_�o�[�W�����l�p�[�X_����n(string raw)
        {
            var success = VmmAppVersion.TryParse(raw, out var result);
            Assert.IsTrue(success);
            Assert.AreEqual(1, result.Major);
            Assert.AreEqual(2, result.Minor);
            Assert.AreEqual(3, result.Build);
        }

        [TestCase("4.6.9.3")]
        [TestCase("4.6.9.abc", Description = "4���ڈȍ~�͐��l����Ȃ��Ă��ʂ�")]
        [Test]
        public void Test_�o�[�W�����l�p�[�X_����n_4���\�L����Ǝ�O��3�����g��(string raw)
        {
            var success = VmmAppVersion.TryParse(raw, out var result);
            Assert.IsTrue(success);
            Assert.AreEqual(4, result.Major);
            Assert.AreEqual(6, result.Minor);
            Assert.AreEqual(9, result.Build);
        }

        [TestCase("")]
        [TestCase(null)]
        [Test]
        public void Test_�o�[�W�����l�p�[�X_�ُ�n_�󕶎���null(string raw)
        {
            var success = VmmAppVersion.TryParse(raw, out var result);
            Assert.IsFalse(success);
            Assert.AreEqual(0, result.Major);
            Assert.AreEqual(0, result.Minor);
            Assert.AreEqual(0, result.Build);
        }

        [TestCase("a1.2.3", Description = "prefix��v�ȊO�_��")]
        [TestCase("1.2.3a", Description = "suffix������̂̓_��")]
        [TestCase("1.xxx.2", Description = "�r���ɕςȒl������ƃ_��")]
        [Test]
        public void Test_�o�[�W�����l�p�[�X_�ُ�n_����������(string raw)
        {
            var success = VmmAppVersion.TryParse(raw, out var result);
            Assert.IsFalse(success);
            Assert.AreEqual(0, result.Major);
            Assert.AreEqual(0, result.Minor);
            Assert.AreEqual(0, result.Build);
        }

        [Test]
        public void Test_�o�[�W�����l�̔�r()
        {
            //���W���[�o�[�W�����Ō��܂�
            var a = new VmmAppVersion(2, 3, 4);
            var b = new VmmAppVersion(1, 5, 11);
            Assert.IsTrue(a.IsNewerThan(b));
            Assert.IsFalse(b.IsNewerThan(a));

            //�}�C�i�[�o�[�W�����Ō��܂�
            b = new VmmAppVersion(2, 1, 5);
            Assert.IsTrue(a.IsNewerThan(b));
            Assert.IsFalse(b.IsNewerThan(a));

            //�r���h�o�[�W�����Ō��܂�
            b = new VmmAppVersion(2, 3, 2);
            Assert.IsTrue(a.IsNewerThan(b));
            Assert.IsFalse(b.IsNewerThan(a));

            //������: �ǂ�����Newer�ł͂Ȃ�
            b = new VmmAppVersion(2, 3, 4);
            Assert.IsFalse(a.IsNewerThan(b));
            Assert.IsFalse(b.IsNewerThan(a));
        }

        [Test]
        public void Test_�o�[�W�����l��Valid�()
        {
            Assert.IsTrue(new VmmAppVersion(0, 0, 1).IsValid);
            Assert.IsTrue(new VmmAppVersion(0, 1, 0).IsValid);
            Assert.IsTrue(new VmmAppVersion(1, 0, 0).IsValid);
            //�S��0����NG
            Assert.IsFalse(new VmmAppVersion(0, 0, 0).IsValid);
        }

        [Test]
        public void Test_�����[�X�m�[�g����n()
        {
            var note = ReleaseNote.FromRawString(
@"2021/10/24

Japanese:

- �ǉ�: hoge.
- �C��: fuga.

English:

- Add: Foo
- Fix: Bar

Note:

- This is note area which should be ignored in parse process.
");

            Assert.AreEqual("2021/10/24", note.DateString);
            Assert.AreEqual("- �ǉ�: hoge.\n- �C��: fuga.", note.JapaneseNote);
            Assert.AreEqual("- Add: Foo\n- Fix: Bar", note.EnglishNote);
        }

        [TestCase("")]
        [TestCase(null)]
        [Test]
        public void Test_�����[�X�m�[�g�ُ�n_�󕶎��Ƃ�null(string rawNote)
        {
            var note = ReleaseNote.FromRawString(rawNote);
            Assert.AreEqual("", note.DateString);
            Assert.AreEqual("", note.JapaneseNote);
            Assert.AreEqual("", note.EnglishNote);
        }

        [TestCase(@"
Japanese:

- �ǉ�: hoge.
- �C��: fuga.

English:

- Add: Foo
- Fix: Bar

Note:

- This is note area which should be ignored in parse process.
")]
        [TestCase(@"2048/May/11

Japanese:

- �ǉ�: hoge.
- �C��: fuga.

English:

- Add: Foo
- Fix: Bar

Note:

- This is note area which should be ignored in parse process.
")]
        [Test]
        public void Test_�����[�X�m�[�g�ُ�n_���t���Ȃ��������ُ�̏ꍇ�͋�(string rawNote)
        {
            var note = ReleaseNote.FromRawString(rawNote);
            Assert.AreEqual("", note.DateString);
            Assert.AreEqual("- �ǉ�: hoge.\n- �C��: fuga.", note.JapaneseNote);
            Assert.AreEqual("- Add: Foo\n- Fix: Bar", note.EnglishNote);
        }

        [TestCase(@"2021/10/24

Japanese

- �ǉ�: hoge.
- �C��: fuga.

English

- Add: Foo
- Fix: Bar

Note:

- This is note area which should be ignored in parse process.
")]
        [TestCase(@"2021/10/24

Japanese

- �ǉ�: hoge.
- �C��: fuga.

English:

- Add: Foo
- Fix: Bar

Note:

- This is note area which should be ignored in parse process.
")]
        [Test]
        public void Test_�����[�X�m�[�g�ُ�n_��؂肪������JP��EN���S������(string rawReleaseNote)
        {
            var note = ReleaseNote.FromRawString(rawReleaseNote);

            //�����ɓ��t������ꍇ�͑S���̂ق��ɓ����Ă�΂����̂ŁADateString������ɂȂ��Ă�̂����A�Ƃ����̂��|�C���g
            Assert.AreEqual("", note.DateString);
            Assert.AreEqual(rawReleaseNote, note.JapaneseNote);
            Assert.AreEqual(rawReleaseNote, note.EnglishNote);
        }
    }
}